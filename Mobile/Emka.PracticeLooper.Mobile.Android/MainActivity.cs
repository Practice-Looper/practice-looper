using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Content;
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
            var adMobId = GlobalApp.ConfigurationService.GetValue("admob:android:id");
            Android.Gms.Ads.MobileAds.Initialize(ApplicationContext, adMobId);
            SQLitePCL.Batteries_V2.Init();
            CrossMediaManager.Current.Init(this);

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
    }
}