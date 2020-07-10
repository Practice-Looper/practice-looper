using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Model.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using static Emka3.PracticeLooper.Config.Secrets;
using Foundation;
using MediaManager;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SpotifyBindings.iOS;
using Syncfusion.SfPicker.XForms.iOS;
using Syncfusion.SfRangeSlider.XForms.iOS;
using UIKit;
using Xamarin.Essentials;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            AppCenter.Start(GetSecrets().AppCenterIos, typeof(Analytics), typeof(Crashes));
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            GlobalApp.Init();
            SQLitePCL.Batteries_V2.Init();
            Rg.Plugins.Popup.Popup.Init();
            Google.MobileAds.MobileAds.SharedInstance.Start(null);
            CrossMediaManager.Current.Init();
            SfPickerRenderer.Init();

            global::Xamarin.Forms.Forms.Init();
            new SfRangeSliderRenderer();
            LoadApplication(new App());
            stopWatch.Stop();
            Analytics.TrackEvent(TrackerEvents.GeneralInformation.ToString(), new Dictionary<string, string>
            {
                { $"Startup time iOS", $"duration {stopWatch.ElapsedMilliseconds} ms" },
                { "OS version", $"{AppInfo.VersionString}" },
                { "Device", $"{DeviceInfo.Manufacturer} {DeviceInfo.Model}" }
            });
            GlobalApp.ConfigurationService.SetValue("Locale", NSLocale.CurrentLocale.LocaleIdentifier);
            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            try
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
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
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
            try
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

                DeviceDisplay.KeepScreenOn = false;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
            }
        }
    }
}
