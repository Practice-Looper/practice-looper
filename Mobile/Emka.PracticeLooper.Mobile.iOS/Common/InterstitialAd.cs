﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Feature;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Google.MobileAds;
using UIKit;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class InterstitialAd : IInterstitialAd
    {
        #region Fields
        Interstitial interstitialAd;
        AutoResetEvent adClosedEvent;
        #endregion

        #region Ctor

        public InterstitialAd()
        {
            if (FeatureRegistry.IsEnabled<IInterstitialAd>())
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
            if (FeatureRegistry.IsEnabled<IInterstitialAd>() && interstitialAd.IsReady)
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

            if (FeatureRegistry.IsEnabled<IInterstitialAd>())
            {
                MainThread.BeginInvokeOnMainThread(LoadAd);
            }
        }

        void LoadAd()
        {
            interstitialAd = new Interstitial(GlobalApp.ConfigurationService.GetValue<string>("AdmobIosInterstitialProjectAdId"));
            var request = Request.GetDefaultRequest();
            request.TestDevices = new[] { Request.SimulatorId.ToString(), "6fb304bbcc401debac41d2255509463f" };
            interstitialAd.LoadRequest(request);
        }

        UIViewController GetVisibleViewController()
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

        void OnAdClosed(object o, EventArgs e)
        {
            adClosedEvent.Set();
        }
        #endregion
    }
}
