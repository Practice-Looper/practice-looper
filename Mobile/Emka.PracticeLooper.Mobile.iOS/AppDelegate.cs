﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
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
using Factory = Emka3.PracticeLooper.Mappings.Factory;
using Emka3.PracticeLooper.Mappings.Contracts;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Config.Contracts;
using Emka.PracticeLooper.Mobile.iOS.Common;
using Plugin.InAppBilling;

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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            Setup();

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
                    var accountMngr = Factory.GetResolver().Resolve<ITokenStorage>();
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
            }
        }

        private void Setup()
        {
            var configService = new ConfigurationService(new PersistentConfigService()) ?? throw new ArgumentNullException("configService");

            string key;
#if PREMIUM
            key = configService.GetValue("AppCenterIosPremium");
            configService.SetValue(PreferenceKeys.PremiumGeneral, true, true);
#else
            key = configService.GetValue("AppCenterIosLite");
#endif
            AppCenter.Start(key, typeof(Analytics), typeof(Crashes));
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(configService.GetValue("SyncFusionLicenseKey"));
            configService.LocalPath = FileSystem.AppDataDirectory;
            var resolver = Factory.GetResolver() ?? throw new ArgumentNullException("resolver");
            resolver.RegisterInstance(configService, typeof(IConfigurationService));
            resolver.RegisterSingleton(typeof(SpotifyLoader), typeof(ISpotifyLoader));
            resolver.Register(typeof(SpotifyAudioPlayer), typeof(IAudioPlayer));
            resolver.Register(typeof(InAppBillingVerifyPurchase), typeof(IInAppBillingVerifyPurchase));
            resolver.RegisterSingleton(typeof(AudioFileRepository), typeof(IFileRepository));
            resolver.RegisterSingleton(typeof(AudioMetadataReader), typeof(IAudioMetadataReader));
            resolver.RegisterSingleton(typeof(InterstitialAd), typeof(IInterstitialAd));
            resolver.RegisterSingleton(typeof(ConnectivityService), typeof(IConnectivityService));
        }
    }
}
