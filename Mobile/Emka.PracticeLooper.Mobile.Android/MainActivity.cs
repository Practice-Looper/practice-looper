//using System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Emka.PracticeLooper.Mobile.Droid.Common;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Common;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Java.Interop;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin.Essentials;
using Xamarin.Forms;
using Factory = Emka3.PracticeLooper.Mappings.Factory;

namespace Emka.PracticeLooper.Mobile.Droid
{
    [Activity(MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Platform.Init(this, savedInstanceState);
            Setup();

            GlobalApp.MainActivity = this;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            SQLitePCL.Batteries_V2.Init();
            Platform.Init(this, savedInstanceState);
            Forms.SetFlags("CarouselView_Experimental");
            Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            base.SetTheme(Resource.Style.MainTheme);

            stopWatch.Stop();

            Analytics.TrackEvent(TrackerEvents.GeneralInformation.ToString(), new Dictionary<string, string>
            {
                { $"Startup time Android", $"duration {stopWatch.ElapsedMilliseconds} ms" },
                { "OS version", $"{AppInfo.VersionString}" },
                { "Device", $"{DeviceInfo.Manufacturer} {DeviceInfo.Model}" }
            });
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
            try
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
                            accountMngr.UpdateAccessToken(AudioSourceType.Spotify, response.AccessToken, int.MaxValue);
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

        private void Setup()
        {
            var configService = new ConfigurationService(new PersistentConfigService(), new JsonConfigLoader(), new SecureConfigService()) ?? throw new ArgumentNullException("configService");
            configService.ReadConfigs("Emka3.PracticeLooper.Config.secrets.json");
            string key;
#if PREMIUM
            key = configService.GetValue("AppCenterAndroidPremium");
#else
            key = configService.GetValue("AppCenterAndroidLite");
#endif
            AppCenter.Start(key, typeof(Analytics), typeof(Crashes));
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(configService.GetValue("SyncFusionLicenseKey"));
            Android.Gms.Ads.MobileAds.Initialize(ApplicationContext, configService.GetValue("AdmobAndroidAppId"));
            configService.SetValue(PreferenceKeys.InternalStoragePath, FileSystem.AppDataDirectory);
            GlobalApp.SpotifyRedirectUrl = configService.GetValue("SpotifyClientRedirectUri");
            var externalDir = GetExternalFilesDirs(Android.OS.Environment.DirectoryMusic).FirstOrDefault(dir => dir.AbsolutePath.Contains("storage/emulated/0"));
            configService.SetValue(PreferenceKeys.ExternalStoragePath, externalDir?.AbsolutePath);

            var resolver = Factory.GetResolver() ?? throw new ArgumentNullException("resolver");
            resolver.RegisterInstance(configService, typeof(IConfigurationService));
            resolver.RegisterSingleton(typeof(InterstitialAd), typeof(IInterstitialAd));
            resolver.RegisterSingleton(typeof(InAppBillingService), typeof(IInAppBillingService));
            resolver.RegisterSingleton(typeof(AudioFileRepository), typeof(IFileRepository));
            resolver.RegisterSingleton(typeof(AudioMetadataReader), typeof(IAudioMetadataReader));
            resolver.Register(typeof(SpotifyAudioPlayer), typeof(IAudioPlayer));
            resolver.RegisterSingleton(typeof(SpotifyLoader), typeof(ISpotifyLoader));
            resolver.RegisterSingleton(typeof(ConnectivityService), typeof(IConnectivityService));
            resolver.RegisterSingleton(typeof(DeviceStorageService), typeof(IDeviceStorageService));
        }

        [Export("GetLanguage")]
        public string GetLanguage()
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return culture.TwoLetterISOLanguageName;
        }
    }
}
