using System;
using System.Linq;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;
using MediaManager;
using Microsoft.AppCenter.Crashes;
using SpotifyBindings.iOS;
using UIKit;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

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
    public class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            GlobalApp.Init();
            SQLitePCL.Batteries_V2.Init();
            Rg.Plugins.Popup.Popup.Init();
            Google.MobileAds.MobileAds.SharedInstance.Start(null);
            CrossMediaManager.Current.Init();

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());
            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var spotifyLoader = Factory.GetResolver().Resolve<ISpotifyLoader>();
            var api = spotifyLoader.RemoteApi as SPTAppRemote;
            NSDictionary authParams = api.AuthorizationParametersFromURL(url);
            var token = authParams[Constants.SPTAppRemoteAccessTokenKey].ToString();

            if (!string.IsNullOrEmpty(token))
            {
                spotifyLoader.Token = token;
                var accountMngr = Factory.GetResolver().Resolve<IAccountManager>();
                accountMngr.UpdateTokenAsync(token).Wait();
            }

            return true;
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Crashes.TrackError(e.Exception);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Crashes.TrackError(e.ExceptionObject as Exception);
        }

        public override void WillTerminate(UIApplication uiApplication)
        {
            base.WillTerminate(uiApplication);
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
    }
}
