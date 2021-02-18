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
using Emka3.PracticeLooper.Services.Contracts.Common;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
namespace Emka.PracticeLooper.Mobile.Droid.Renderers
{
    [Preserve(AllMembers = true)]
    public class CustomWebViewRenderer : WebViewRenderer
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

        public CustomWebViewRenderer(Context context) : base(context)
        {
            configService = Factory.GetResolver().Resolve<IConfigurationService>();
            logger = Factory.GetResolver().Resolve<ILogger>();
            spotifyWebPlayerUri = configService.GetValue("SpotifyPlayerUri");
            loadPlayerTimeout = TimeSpan.FromSeconds(30);
            activatePlayerTimeout = TimeSpan.FromSeconds(30);
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
                Element.Navigated += OnNavigationCompleted;
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

            await Device.InvokeOnMainThreadAsync(() => ActivatePlayerInternal());
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

        private void ActivatePlayerInternal()
        {
            hasBeenActivated = false;

            try
            {
                if (hasBeenLoaded)
                {
                    var script = "document.querySelector(\"div[data-test-id='play-pause-button']\").click()";
                    LoadUrl($"javascript: {script}");
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

        private void OnNavigating(object sender, WebNavigatingEventArgs e)
        {
            if (!e.Url.Contains("javascript"))
            {
                MessagingCenter.Send<object>(this, MessengerKeys.WebViewNavigating);
            }
        }

        private void OnNavigationCompleted(object sender, WebNavigatedEventArgs e)
        {
            var navigationResult = e.Result == WebNavigationResult.Success;

            try
            {
                hasBeenLoaded = false;
                hasBeenActivated = false;
                Task.Run(() => navigationCompletedTaskSource?.TrySetResult(navigationResult));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
            }
            finally
            {
                MessagingCenter.Send<object, bool>(this, MessengerKeys.WebViewNavigated, navigationResult);
            }
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
