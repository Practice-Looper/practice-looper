// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2021

using System;
using System.Threading;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.iOS.Renderer;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Mobile.Views;
using Emka.PracticeLooper.Model;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Mappings;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
namespace Emka.PracticeLooper.Mobile.iOS.Renderer
{
    public class CustomWebViewRenderer : WkWebViewRenderer
    {
        private readonly IConfigurationService configService;
        private string spotifyWebPlayerUri;
        private bool hasBeenActivated;
        private bool hasBeenLoaded;
        private AutoResetEvent navigationCompletedEvent;
        private AutoResetEvent loadingCompletedEvent;
        private AutoResetEvent activationCompletedEvent;
        private TimeSpan timeOut;
        public CustomWebViewRenderer()
        {
            configService = Factory.GetResolver().Resolve<IConfigurationService>();
            spotifyWebPlayerUri = configService.GetValue("SpotifyPlayerUri");
            timeOut = TimeSpan.FromSeconds(10);
            MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.SpotifyActivatePlayer, OnActivatePlayer);
            MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.SpotifyLoadWebPlayer, OnLoadWebPlayer);
        }

        protected override async void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            var navigationDelegate = new CustomNavigationDelegate();

            navigationDelegate.NavigatiomCompleted += OnNavigationCompleted;
            navigationDelegate.Navigating += OnNavigating;

            NavigationDelegate = navigationDelegate;
            var userAgentEvent = new AutoResetEvent(false);
            string userAgent = null;

            if (e.NewElement != null)
            {
                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    EvaluateJavaScript("navigator.userAgent", (result, error) =>
                    {
                        if (result != null)
                        {
                            userAgent = result.ToString();
                            userAgent = userAgent.Replace("iPad", "iPhone");
                        }

                        userAgentEvent.Set();
                    });
                }

                await Task.Run(() => userAgentEvent.WaitOne(TimeSpan.FromSeconds(10)));
                CustomUserAgent = userAgent;
                await LoadPlayerInternal();
                MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyWebPlayerLoaded, hasBeenLoaded);
            }
        }

        private async void OnActivatePlayer(MainViewModel obj)
        {
            await ActivatePlayerInternal();
            activationCompletedEvent?.WaitOne(TimeSpan.FromSeconds(60));
            MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyPlayerActivated, hasBeenActivated);
        }

        private void OnNavigating(object sender, bool isNavigating)
        {
            MessagingCenter.Send<object, bool>(this, MessengerKeys.WebViewNavigationStatus, isNavigating);
        }

        private void OnNavigationCompleted(object sender, bool success)
        {
            navigationCompletedEvent?.Set();
        }

        private async void OnLoadWebPlayer(MainViewModel obj)
        {
            await LoadPlayerInternal();
            loadingCompletedEvent.WaitOne(timeOut);
            MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyWebPlayerLoaded, hasBeenLoaded);
        }

        private async Task LoadPlayerInternal()
        {
            loadingCompletedEvent = new AutoResetEvent(false);
            navigationCompletedEvent = new AutoResetEvent(false);
            LoadUrl(spotifyWebPlayerUri); // load web player

            try
            {
                if (Element is CustomWebView)
                {
                    await Task.Run(navigationCompletedEvent.WaitOne); // wait until navigation completes
                    int retries = 3;
                    while (!hasBeenLoaded || retries > 0)
                    {
                        try
                        {
                            // check whether login page or player has been loaded
                            var scriptResult = await EvaluateJavaScriptAsync("document.querySelector(\"div[data-test-id='play-pause-button']\").toString()");
                            hasBeenLoaded = !string.IsNullOrWhiteSpace(scriptResult?.ToString());

                            if (hasBeenLoaded)
                            {
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            retries--;
                            await Task.Delay(TimeSpan.FromSeconds(2));
                            continue;
                        }
                    }

                    loadingCompletedEvent?.Set();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task ActivatePlayerInternal()
        {
            activationCompletedEvent = new AutoResetEvent(false);

            if (hasBeenActivated)
            {
                activationCompletedEvent?.Set();
                return;
            }

            try
            {
                await EvaluateJavaScriptAsync("document.querySelector(\"div[data-test-id='play-pause-button']\").click()");
                hasBeenActivated = true;
            }
            catch (Exception)
            {
                hasBeenActivated = false;
            }

            activationCompletedEvent?.Set();
        }
    }

    public class CustomNavigationDelegate : WKNavigationDelegate
    {
        public event EventHandler<bool> NavigatiomCompleted;
        public event EventHandler<bool> Navigating;

        public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            Navigating?.Invoke(this, true);
        }

        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            Navigating?.Invoke(this, false);
            NavigatiomCompleted?.Invoke(this, true);
        }

        public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, Foundation.NSError error)
        {
            Navigating?.Invoke(this, false);
            NavigatiomCompleted?.Invoke(this, false);
        }
    }
}
