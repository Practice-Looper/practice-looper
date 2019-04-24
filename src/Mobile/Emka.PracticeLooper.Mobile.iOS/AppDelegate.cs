using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Emka.PracticeLooper.Mobile.iOS.Delegates;
using Foundation;
using SpotifyBindings.iOS;
using UIKit;

//#if NETFX_CORE
//[assembly: Xamarin.Forms.Platform.WinRT.ExportRenderer(typeof(Xamarin.RangeSlider.Forms.RangeSlider), typeof(Xamarin.RangeSlider.Forms.RangeSliderRenderer))]
//#else
//[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarin.RangeSlider.Forms.RangeSlider), typeof(Xamarin.RangeSlider.Forms.RangeSliderRenderer))]
//#endif
//[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarin.RangeSlider.Forms.RangeSlider), typeof(Xamarin.RangeSlider.Forms.RangeSliderRenderer))]
namespace Emka.PracticeLooper.Mobile.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private SPTAppRemote api;
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            GlobalApp.Init();
            SQLitePCL.Batteries_V2.Init();
            Rg.Plugins.Popup.Popup.Init();
            var adMobId = GlobalApp.ConfigurationService.GetValue("admob:ios:id");

            var clientId = GlobalApp.ConfigurationService.GetValue("auth:spotify:client:id");
            var redirectUri = GlobalApp.ConfigurationService.GetValue("auth:spotify:client:uri:redirect");

            var appConfig = new SPTConfiguration(clientId, NSUrl.FromString(redirectUri));
            api = new SPTAppRemote(appConfig, SPTAppRemoteLogLevel.Error);

            SPTAppRemote.CheckIfSpotifyAppIsActive((obj) =>
            {
                Console.WriteLine(obj);
            });

            var isSpotifyInstalled = api.AuthorizeAndPlayURI(string.Empty);
            //api.


            //api.PlayerAPI.Delegate = new SpotifyAppRemotePlayerStateDelegate();
            //api.PlayerAPI.SubscribeToPlayerState((NSObject arg0, NSError arg1) =>
            //{
            //    Console.WriteLine(arg0);
            //});
            Google.MobileAds.MobileAds.Configure(adMobId);
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            NSDictionary authParams = api.AuthorizationParametersFromURL(url);
            var token = authParams[Constants.SPTAppRemoteAccessTokenKey].ToString();

            if (!string.IsNullOrEmpty(token))
            {
                api.ConnectionParameters.AccessToken = token;
            }

            api.Delegate = new SpotifyAppRemoteDelegate();
            api.Connect();

            return true;
        }
    }
}
