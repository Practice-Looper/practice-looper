// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Foundation;
using Google.MobileAds;
using UIKit;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    [Preserve(AllMembers = true)]
    public class InterstitialAd : IInterstitialAd
    {
        #region Fields
        Interstitial interstitialAd;
        AutoResetEvent adClosedEvent;
        private readonly ILogger logger;
        private readonly IConfigurationService configurationService;
        #endregion

        #region Ctor

        public InterstitialAd(ILogger logger, IConfigurationService configurationService)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (!configurationService.GetSecureValue<bool>(PreferenceKeys.PremiumGeneral))
            {
                LoadAd();
            }
        }
        #endregion

        #region Methods

        public void ShowAd()
        {
            if (interstitialAd.IsReady)
            {
                var viewController = GetVisibleViewController();
                interstitialAd.Present(viewController);
            }
        }

        public async Task ShowAdAsync()
        {
            // todo: block if ad is loading...
            if (!configurationService.GetSecureValue<bool>(PreferenceKeys.PremiumGeneral) && interstitialAd.IsReady)
            {
                adClosedEvent = new AutoResetEvent(false);
                await Task.Run(() =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var viewController = GetVisibleViewController();

                        interstitialAd.ScreenDismissed += OnAdClosed;
                        interstitialAd.Present(viewController);
                    });

                    adClosedEvent.WaitOne();
                });
            }

            if (!configurationService.GetSecureValue<bool>(PreferenceKeys.PremiumGeneral))
            {
                MainThread.BeginInvokeOnMainThread(LoadAd);
            }
        }

        void LoadAd()
        {
            try
            {
                interstitialAd = new Interstitial(configurationService.GetValue<string>("AdmobIosInterstitialProjectAdId"));
#if DEBUG
                MobileAds.SharedInstance.RequestConfiguration.TestDeviceIdentifiers = new[] { "6fb304bbcc401debac41d2255509463f", "3408f4ee8f8d77f2efcbf255ead140d6" };
#endif
                interstitialAd.FailedToPresentScreen += OnFailedToPresentScreen;
                interstitialAd.ReceiveAdFailed += OnReceiveAdFailed;
                var request = Request.GetDefaultRequest();
                interstitialAd.LoadRequest(request);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        private async void OnReceiveAdFailed(object sender, InterstitialDidFailToReceiveAdWithErrorEventArgs e)
        {
            await logger?.LogErrorAsync(new Exception(e.Error.Description));
        }

        private async void OnFailedToPresentScreen(object sender, EventArgs e)
        {
            await logger?.LogErrorAsync(new Exception("Failed to present add screen"));
        }

        UIViewController GetVisibleViewController()
        {
            try
            {
                var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

                if (rootController.PresentedViewController == null)
                    return rootController;

                if (rootController.PresentedViewController is UINavigationController)
                {
                    return ((UINavigationController)rootController.PresentedViewController).VisibleViewController;
                }

                if (rootController.PresentedViewController is UITabBarController)
                {
                    return ((UITabBarController)rootController.PresentedViewController).SelectedViewController;
                }

                return rootController.PresentedViewController;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        void OnAdClosed(object o, EventArgs e)
        {
            adClosedEvent.Set();
        }
        #endregion
    }
}
