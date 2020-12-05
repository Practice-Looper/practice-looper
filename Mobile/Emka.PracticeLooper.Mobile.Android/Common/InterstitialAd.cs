// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Ads;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Config.Contracts.Features;
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
        private readonly IFeatureRegistry featureRegistry;
        #endregion

        #region Events

        public event EventHandler AdLoaded;
        public event EventHandler AdClosed;
        public event EventHandler AdOpened;
        #endregion

        #region Ctor

        public InterstitialAd(ILogger logger, IConfigurationService configurationService, IFeatureRegistry featureRegistry)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.featureRegistry = featureRegistry ?? throw new ArgumentNullException(nameof(featureRegistry));
            interstitialAd = new Android.Gms.Ads.InterstitialAd(Application.Context);
            interstitialAd.RewardedVideoAdFailedToLoad += OnRewardedVideoAdFailedToLoad;
            interstitialAd.AdListener = this;
            interstitialAd.AdUnitId = configurationService?.GetValue<string>("AdmobAndroidInterstitialProjectAdId");

            if (!featureRegistry.IsEnabled<PremiumFeature>())
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
            if (!featureRegistry.IsEnabled<PremiumFeature>() && interstitialAd.IsLoaded)
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

            if (!featureRegistry.IsEnabled<PremiumFeature>())
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
                requestbuilder.AddTestDevice("B0B92426A54BF08C56BC62D5703161F3");
                requestbuilder.AddTestDevice("A808EFF8867E3C5741E6456A4FFAB5D4");
                requestbuilder.AddTestDevice("7E7FE7BDD31E1B60AAA1ABA764B55E8B");
                requestbuilder.AddTestDevice("A3DC0ED5802A0E68DEC7B1AC713CB73A");

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

        public void Toggle(bool enabled)
        {
            if (enabled)
            {
                LoadAd();
            }
        }
        #endregion
    }
}
