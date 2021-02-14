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
using Emka3.PracticeLooper.Services.Contracts.Common;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
namespace Emka.PracticeLooper.Mobile.iOS.Renderer
{
    public class CustomWebViewRenderer : WkWebViewRenderer
    {
        private readonly IConfigurationService configService;
        private readonly ILogger logger;
        private string spotifyWebPlayerUri;
        private bool hasBeenActivated;
        private bool hasBeenLoaded;
        private TaskCompletionSource<bool> navigationCompletedTaskSource;
        private TaskCompletionSource<bool> loadingCompletedTaskSource;
        private TaskCompletionSource<bool> activationCompletedTaskSource;
        private TimeSpan loadPlayerTimeout;
        private TimeSpan activatePlayerTimeout;

        public CustomWebViewRenderer()
        {
            configService = Factory.GetResolver().Resolve<IConfigurationService>();
            logger = Factory.GetResolver().Resolve<ILogger>();
            spotifyWebPlayerUri = configService.GetValue("SpotifyPlayerUri");
            loadPlayerTimeout = TimeSpan.FromSeconds(30);
            activatePlayerTimeout = TimeSpan.FromSeconds(30);
            MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.SpotifyActivatePlayer, OnActivatePlayer);
            MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.SpotifyLoadWebPlayer, OnLoadWebPlayer);
        }

        protected override async void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            var navigationDelegate = new CustomNavigationDelegate();

            navigationDelegate.NavigationCompleted += OnNavigationCompleted;
            navigationDelegate.Navigating += OnNavigating;

            NavigationDelegate = navigationDelegate;
            string userAgent = null;

            if (e.NewElement != null)
            {
                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    userAgent = ValueForKey(new Foundation.NSString("userAgent"))?.ToString();
                    if (!string.IsNullOrWhiteSpace(userAgent))
                    {
                        userAgent = userAgent.Replace("iPad", "iPhone");
                        CustomUserAgent = userAgent;
                    }
                }

                await LoadPlayerInternal();
                MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyWebPlayerLoaded, hasBeenLoaded);
            }
        }

        private async void OnLoadWebPlayer(MainViewModel obj)
        {
            var loadingCancelToken = new CancellationTokenSource(loadPlayerTimeout);
            loadingCompletedTaskSource = new TaskCompletionSource<bool>();
            loadingCancelToken.Token.Register(() => loadingCompletedTaskSource.TrySetResult(false), false);

            await LoadPlayerInternal();
            await loadingCompletedTaskSource.Task;
            MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyWebPlayerLoaded, hasBeenLoaded);
        }

        private async void OnActivatePlayer(MainViewModel obj)
        {

            var activationCancelToken = new CancellationTokenSource(activatePlayerTimeout);
            activationCompletedTaskSource = new TaskCompletionSource<bool>();
            activationCancelToken.Token.Register(() => { activationCompletedTaskSource.TrySetResult(false); });

            await ActivatePlayerInternal();
            await activationCompletedTaskSource?.Task;
            MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyPlayerActivated, hasBeenActivated);
        }

        private async Task LoadPlayerInternal()
        {
            try
            {
                hasBeenLoaded = false;
                var navigationCancelToken = new CancellationTokenSource(loadPlayerTimeout);
                navigationCompletedTaskSource = new TaskCompletionSource<bool>();
                navigationCancelToken.Token.Register(() => navigationCompletedTaskSource.TrySetResult(false), false);

                LoadUrl(spotifyWebPlayerUri); // load web player

                if (Element is CustomWebView)
                {
                    var navigationSuccess = await navigationCompletedTaskSource.Task; // wait until navigation completes

                    if (navigationSuccess)
                    {
                        int retries = 3;
                        while (!hasBeenLoaded && retries > 0)
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
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.LogWarningAsync(ex.Message);
            }
            finally
            {
                loadingCompletedTaskSource?.TrySetResult(hasBeenLoaded);
            }
        }

        private async Task ActivatePlayerInternal()
        {
            hasBeenActivated = false;

            try
            {
                if (hasBeenLoaded)
                {
                    await EvaluateJavaScriptAsync("document.querySelector(\"div[data-test-id='play-pause-button']\").click()");
                    hasBeenActivated = true;
                }
            }
            catch (Exception)
            {
                hasBeenActivated = false;
            }
            finally
            {
                activationCompletedTaskSource?.TrySetResult(hasBeenLoaded);
            }
        }

        private void OnNavigating(object sender, bool isNavigating)
        {
            MessagingCenter.Send<object>(this, MessengerKeys.WebViewNavigating);
        }

        private void OnNavigationCompleted(object sender, bool success)
        {
            try
            {
                Task.Run(() => navigationCompletedTaskSource?.TrySetResult(success));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
            }
            finally
            {
                MessagingCenter.Send<object, bool>(this, MessengerKeys.WebViewNavigated, success);
            }
        }
    }

    public class CustomNavigationDelegate : WKNavigationDelegate
    {
        public event EventHandler<bool> NavigationCompleted;
        public event EventHandler<bool> Navigating;

        public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            Navigating?.Invoke(this, true);
        }

        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            NavigationCompleted?.Invoke(this, true);
        }

        public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, Foundation.NSError error)
        {
            NavigationCompleted?.Invoke(this, false);
        }

        public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, Foundation.NSError error)
        {
            NavigationCompleted?.Invoke(this, false);
        }
    }
}
