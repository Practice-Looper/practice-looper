using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.iOS.Delegates;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Common;
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
            Google.MobileAds.MobileAds.Configure(adMobId);
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            NSDictionary authParams = GlobalApp.SPTRemoteApi.AuthorizationParametersFromURL(url);
            var token = authParams[Constants.SPTAppRemoteAccessTokenKey].ToString();

            if (!string.IsNullOrEmpty(token))
            {
                GlobalApp.SPTRemoteApi.ConnectionParameters.AccessToken = token;
                var accountMngr = Factory.GetResolver().Resolve<IAccountManager>();
                accountMngr.UpdateTokenAsync(token).Wait();
                GlobalApp.SpotifyTokenCompletionSource.SetResult(true);
            }

            GlobalApp.SPTRemoteApi.Delegate = new SpotifyAppRemoteDelegate();
            GlobalApp.SPTRemoteApi.Connect();

            return true;
        }
    }
}
