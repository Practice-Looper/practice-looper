using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Content;
using Emka.PracticeLooper.Mobile.Droid.Helpers;
using Emka3.PracticeLooper.Services.Contracts.Common;
using MediaManager;

namespace Emka.PracticeLooper.Mobile.Droid
{
    [Activity(Label = "Emka.PracticeLooper.Mobile", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            GlobalApp.Init();
            GlobalApp.MainActivity = this;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
            {
                var mounted = Environment.ExternalStorageState == Environment.MediaMounted;
                GlobalApp.HasPermissionToWriteExternalStorage = mounted;
            }
            else
            {
                // Todo: request permisstions
            }

            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            var adMobId = Secrets.AdmobAndroidAppId;
            Android.Gms.Ads.MobileAds.Initialize(ApplicationContext, adMobId);
            SQLitePCL.Batteries_V2.Init();
            CrossMediaManager.Current.Init(this);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
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
            if (requestCode == 7737)
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
                        // Handle error response
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