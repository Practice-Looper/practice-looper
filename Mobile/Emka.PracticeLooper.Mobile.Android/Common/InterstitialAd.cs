// Copyright (C)  - All Rights Reserved
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
        private readonly IConfigurationService configurationService;
        #endregion

        #region Events

        public event EventHandler AdLoaded;
        public event EventHandler AdClosed;
        public event EventHandler AdOpened;
        #endregion

        #region Ctor

        public InterstitialAd(ILogger logger, IConfigurationService configurationService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            interstitialAd = new Android.Gms.Ads.InterstitialAd(Application.Context);
            interstitialAd.RewardedVideoAdFailedToLoad += OnRewardedVideoAdFailedToLoad;
            interstitialAd.AdListener = this;
            interstitialAd.AdUnitId = GlobalApp.ConfigurationService.GetValue<string>("AdmobAndroidInterstitialProjectAdId");

            if (!configurationService.GetValue<bool>(PreferenceKeys.PremiumGeneral))
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
            if (!configurationService.GetValue<bool>(PreferenceKeys.PremiumGeneral) && interstitialAd.IsLoaded)
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

            if (!configurationService.GetValue<bool>(PreferenceKeys.PremiumGeneral))
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
#if DEBUG
                requestbuilder.AddTestDevice("7E7FE7BDD31E1B60AAA1ABA764B55E8B");
                requestbuilder.AddTestDevice("05EAB5D891232107FF441DD3903FD547");
#endif

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
