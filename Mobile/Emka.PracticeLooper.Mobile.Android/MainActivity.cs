
using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Content;
using Com.Spotify.Android.Appremote.Api;
using Java.Interop;
using Java.Lang;
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
                var mounted = Android.OS.Environment.ExternalStorageState == Android.OS.Environment.MediaMounted;
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

            ConnectionParams connectionParams = new ConnectionParams
            .Builder("74182adb6d774f8498b19efea31d9fac")
            .SetRedirectUri("loopr://callback")
            .ShowAuthView(true)
            .Build();

            SpotifyAppRemote.Connect(this, connectionParams, new ConnectionListenerInternal());


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

    public class ConnectionListenerInternal : Java.Lang.Object, IConnectorConnectionListener
    {

        public void Disposed()
        {
        }

        public void DisposeUnlessReferenced()
        {
        }

        public void Finalized()
        {
        }

        public void OnConnected(SpotifyAppRemote p0)
        {
            Console.WriteLine(p0.IsConnected);
            p0.PlayerApi.Play("spotify:playlist:37i9dQZF1DX2sUQwD7tbmL");
        }

        public void OnFailure(Throwable p0)
        {
            Console.WriteLine(p0.Message);
        }

        public void SetJniIdentityHashCode(int value)
        {
        }

        public void SetJniManagedPeerState(JniManagedPeerStates value)
        {
        }

        public void SetPeerReference(JniObjectReference reference)
        {
        }
    }
}