// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2021
using System.Threading;
using System;
using Android.Content;
using Emka.PracticeLooper.Mobile.Droid.Common;
using Emka.PracticeLooper.Mobile.Droid.Renderers;
using Emka.PracticeLooper.Mobile.Views;
using Emka3.PracticeLooper.Config.Contracts;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;
using Emka3.PracticeLooper.Mappings;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Model;
using System.Threading.Tasks;
using Android.Webkit;
using System.Diagnostics;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
namespace Emka.PracticeLooper.Mobile.Droid.Renderers
{
    [Preserve(AllMembers = true)]
    public class CustomWebViewRenderer : WebViewRenderer
    {
        private readonly IConfigurationService configService;
        private string spotifyWebPlayerUri;
        private bool hasBeenActivated;
        private bool hasBeenLoaded;
        private AutoResetEvent navigationCompletedEvent;
        private AutoResetEvent loadingCompletedEvent;
        private AutoResetEvent activationCompletedEvent;
        private TimeSpan timeOut;

        public CustomWebViewRenderer(Context context) : base(context)
        {
            configService = Factory.GetResolver().Resolve<IConfigurationService>();
            spotifyWebPlayerUri = configService.GetValue("SpotifyPlayerUri");
            timeOut = TimeSpan.FromSeconds(10);
            MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.SpotifyActivatePlayer, OnActivatePlayer);
            MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.SpotifyLoadWebPlayer, OnLoadWebPlayer);
        }

        protected override async void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Settings.JavaScriptEnabled = true;
                Control.Settings.AllowContentAccess = true;
                Control.Settings.SaveFormData = true;
                var spotifyWebViewClient = new SpotifyWebViewClient();
                Control.SetWebChromeClient(spotifyWebViewClient);

                Element.Navigating += OnNavigating;
                Element.Navigated += OnNavigated;
                await LoadPlayerInternal();
                MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyWebPlayerLoaded, hasBeenLoaded);
                //var tcs = new TaskCompletionSource<bool>();
                //Control.EvaluateJavascript("document.documentElement.outerHTML.toString()", new JavaScriptCallback(tcs));
                //await tcs.Task;
            }
        }

        private void OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            MessagingCenter.Send<object, bool>(this, MessengerKeys.WebViewNavigationStatus, false);
            navigationCompletedEvent?.Set();
        }

        private void OnNavigating(object sender, WebNavigatingEventArgs e)
        {
            MessagingCenter.Send<object, bool>(this, MessengerKeys.WebViewNavigationStatus, true);
        }

        private async void OnActivatePlayer(MainViewModel obj)
        {
            await ActivatePlayerInternal();
            activationCompletedEvent?.WaitOne(TimeSpan.FromSeconds(60));
            MessagingCenter.Send<object, bool>(this, MessengerKeys.SpotifyPlayerActivated, hasBeenActivated);
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
                    while (retries != 0)
                    {
                        try
                        {
                            // check whether login page or player has been loaded
                            var completionSource = new TaskCompletionSource<bool>();
                            Control.EvaluateJavascript("document.querySelector(\"div[data-test-id='play-pause-button']\").toString()", new JavaScriptCallback(completionSource));
                            hasBeenLoaded = await completionSource.Task;

                            if (hasBeenLoaded)
                            {
                                break;
                            }

                            retries--;
                            await Task.Delay(TimeSpan.FromSeconds(2));
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
                var script = "document.querySelector(\"div[data-test-id='play-pause-button']\").click()";
                LoadUrl($"javascript: {script}");
                hasBeenActivated = true;
            }
            catch (Exception)
            {
                hasBeenActivated = false;
            }

            activationCompletedEvent?.Set();
        }
    }

    public class JavaScriptCallback : Java.Lang.Object, IValueCallback
    {
        private readonly TaskCompletionSource<bool> completionSource;

        public JavaScriptCallback(TaskCompletionSource<bool> completionSource)
        {
            this.completionSource = completionSource;
        }

        public void OnReceiveValue(Java.Lang.Object value)
        {
            completionSource?.SetResult(value != null && value.ToString() != "null");
        }
    }
}
