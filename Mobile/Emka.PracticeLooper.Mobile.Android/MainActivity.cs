//using System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Media.Session;
using Factory = Emka3.PracticeLooper.Mappings.Factory;
using Emka3.PracticeLooper.Mappings.Contracts;
using Emka3.PracticeLooper.Model.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using MediaManager;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Plugin.InAppBilling;
using Xamarin.Essentials;
using Emka3.PracticeLooper.Config.Contracts;

namespace Emka.PracticeLooper.Mobile.Droid
{
    [Activity(MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            base.OnCreate(savedInstanceState);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            GlobalApp.Init();
            CrossMediaManager.Current.Init(this);
            GlobalApp.MainActivity = this;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
            {
                var mounted = Android.OS.Environment.ExternalStorageState == Android.OS.Environment.MediaMounted;
                GlobalApp.HasPermissionToWriteExternalStorage = mounted;
            }
            else
            {
                // Todo: request permisstions
            }

            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            SQLitePCL.Batteries_V2.Init();
            Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            Android.Gms.Ads.MobileAds.Initialize(ApplicationContext, App.ConfigurationService.GetValue("AdmobAndroidAppId"));
            base.SetTheme(Resource.Style.MainTheme);
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = Platform.CurrentActivity;

            stopWatch.Stop();
            Analytics.TrackEvent(TrackerEvents.GeneralInformation.ToString(), new Dictionary<string, string>
            {
                { $"Startup time Android", $"duration {stopWatch.ElapsedMilliseconds} ms" },
                { "OS version", $"{AppInfo.VersionString}" },
                { "Device", $"{DeviceInfo.Manufacturer} {DeviceInfo.Model}" }
            });

        }

        protected override void OnDestroy()
        {
            try
            {
                base.OnDestroy();
                IResolver resolver = Factory.GetResolver();
                var audioPlayers = resolver.ResolveAll<IAudioPlayer>();
                var spotifyLoader = resolver.Resolve<ISpotifyLoader>();

                if (audioPlayers != null && audioPlayers.Any())
                {
                    foreach (var player in audioPlayers)
                    {
                        player.Pause();
                    }
                }

                if (spotifyLoader != null)
                {
                    spotifyLoader.Disconnect();
                }

                DeviceDisplay.KeepScreenOn = false;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                throw;
            }
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Crashes.TrackError(e.Exception);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Crashes.TrackError(e.ExceptionObject as Exception);
        }

        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
            }
            else
            {
                // Do something if there are not any pages in the `PopupStack`
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
            // Check if result comes from the correct activity
            try
            {
                if (requestCode == Factory.GetResolver().Resolve<IConfigurationService>()?.GetValue<int>("SpotifyClientRequestCode"))
                {
                    var spotifyLoader = Factory.GetResolver().Resolve<ISpotifyLoader>();
                    Com.Spotify.Sdk.Android.Auth.AuthorizationResponse response = Com.Spotify.Sdk.Android.Auth.AuthorizationClient.GetResponse((int)resultCode, data);

                    var type = response.GetType().ToString();

                    switch (type)
                    {
                        // Response was successful and contains auth token
                        case "token":
                            // Handle successful response

                            if (!string.IsNullOrEmpty(response.AccessToken))
                            {
                                var accountMngr = Factory.GetResolver().Resolve<ITokenStorage>();
                                accountMngr.UpdateTokenAsync(response.AccessToken).Wait();
                                spotifyLoader.Token = response.AccessToken;
                            }
                            break;

                        // Auth flow returned an error
                        case "error":

                            if (response.Error == "AUTHENTICATION_DENIED_BY_USER")
                            {
                                Com.Spotify.Sdk.Android.Auth.AuthorizationClient.StopLoginActivity(GlobalApp.MainActivity, requestCode);
                                spotifyLoader.Token = string.Empty;
                            }
                            else
                            {

                                Crashes.TrackError(new Exception(response.Error));
                            }

                            break;
                        default:
                            Com.Spotify.Sdk.Android.Auth.AuthorizationClient.StopLoginActivity(GlobalApp.MainActivity, requestCode);
                            Analytics.TrackEvent(TrackerEvents.SpotifyAuthentication.ToString(), new Dictionary<string, string>
                            {
                                { "ActivityResult", response.GetType().ToString() }
                            });
                            spotifyLoader.Token = string.Empty;
                            break;
                            // Most likely auth flow was cancelled
                            // Handle other cases
                    }
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    public class MediaSessionCallback : MediaSessionCompat.Callback
    {
        public override void OnPause()
        {
            base.OnPause();
        }

        public override void OnPlay()
        {
            base.OnPlay();
        }

        public override void OnSkipToNext()
        {
            base.OnSkipToNext();
        }

        public override void OnSkipToPrevious()
        {
            base.OnSkipToPrevious();
        }

        public override void OnStop()
        {
            base.OnStop();
        }
    }
}