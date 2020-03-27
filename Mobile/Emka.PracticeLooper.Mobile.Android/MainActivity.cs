//using System;
using System;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Content;
using Emka.PracticeLooper.Mobile.Droid.Helpers;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Microsoft.AppCenter.Crashes;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.Droid
{
    [Activity(Label = "Practice Looper", Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            GlobalApp.Init();
            MediaManager.CrossMediaManager.Current.Init();
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
            Android.Gms.Ads.MobileAds.Initialize(ApplicationContext, Secrets.AdmobAndroidAppId);
            SQLitePCL.Batteries_V2.Init();
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MappingsFactory.Contracts.IResolver resolver = Factory.GetResolver();
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

            // Check if result comes from the correct activity
            if (requestCode == GlobalApp.ConfigurationService.GetValue<int>("SpotifyClientRequestCode"))
            {
                Com.Spotify.Sdk.Android.Auth.AuthorizationResponse response = Com.Spotify.Sdk.Android.Auth.AuthorizationClient.GetResponse((int)resultCode, data);
                var type = response.GetType().ToString();

                switch (type)
                {
                    // Response was successful and contains auth token
                    case "token":
                        // Handle successful response

                        if (!string.IsNullOrEmpty(response.AccessToken))
                        {
                            var accountMngr = Emka3.PracticeLooper.Mappings.Factory.GetResolver().Resolve<IAccountManager>();
                            accountMngr.UpdateTokenAsync(response.AccessToken).Wait();
                        }
                        break;

                    // Auth flow returned an error
                    case "error":
                        Crashes.TrackError(new Exception(response.Error));
                        break;

                        // Most likely auth flow was cancelled
                        // Handle other cases
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}