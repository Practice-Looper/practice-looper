﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Ads;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Config.Feature;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class InterstitialAd : AdListener, IInterstitialAd
    {
        #region Fields
        AutoResetEvent adClosedEvent;
        Android.Gms.Ads.InterstitialAd interstitialAd;
        private readonly ILogger logger;
        #endregion

        #region Events

        public event EventHandler AdLoaded;
        public event EventHandler AdClosed;
        public event EventHandler AdOpened;
        #endregion

        #region Ctor

        public InterstitialAd(ILogger logger)
        {
            this.logger = logger;
            interstitialAd = new Android.Gms.Ads.InterstitialAd(Application.Context);
            interstitialAd.RewardedVideoAdFailedToLoad += OnRewardedVideoAdFailedToLoad;
            interstitialAd.AdListener = this;
            interstitialAd.AdUnitId = GlobalApp.ConfigurationService.GetValue<string>("AdmobAndroidInterstitialProjectAdId");

            if (!Factory.GetConfigService().GetValue<bool>(PreferenceKeys.PremiumGeneral))
            {
                LoadAd();
            }
        }

        private async void OnRewardedVideoAdFailedToLoad(object sender, Android.Gms.Ads.Reward.RewardedVideoAdFailedToLoadEventArgs e)
        {
            await logger?.LogErrorAsync(new Exception($"Failed to load add {e.ErrorCode}"));
        }
        #endregion

        #region Methods

        public async Task ShowAdAsync()
        {
            if (!Factory.GetConfigService().GetValue<bool>(PreferenceKeys.PremiumGeneral) && interstitialAd.IsLoaded)
            {
                adClosedEvent = new AutoResetEvent(false);
                await Task.Run(() =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ShowAd();
                    });

                    adClosedEvent.WaitOne();
                });
            }

            if (!Factory.GetConfigService().GetValue<bool>(PreferenceKeys.PremiumGeneral))
            {
                MainThread.BeginInvokeOnMainThread(LoadAd);
            }
        }

        public void ShowAd()
        {
            if (interstitialAd.IsLoaded)
            {
                interstitialAd.Show();
            }

            LoadAd();
        }

        void LoadAd()
        {
            try
            {
                var requestbuilder = new AdRequest.Builder();
                requestbuilder.AddTestDevice("F05EA53323F18138CB1722DD1F00A0F0");
                interstitialAd.LoadAd(requestbuilder.Build());
            }
            catch (Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        public override void OnAdLoaded()
        {
            AdLoaded?.Invoke(this, new EventArgs());
            base.OnAdLoaded();
        }
        public override void OnAdClosed()
        {
            base.OnAdClosed();
            AdClosed?.Invoke(this, new EventArgs());
            adClosedEvent.Set();
        }
        public override void OnAdOpened()
        {
            AdOpened?.Invoke(this, new EventArgs());
            base.OnAdOpened();
        }
        #endregion
    }
}
