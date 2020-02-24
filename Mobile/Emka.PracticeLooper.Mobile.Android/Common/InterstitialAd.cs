// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Ads;
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
        #endregion

        #region Events

        public event EventHandler AdLoaded;
        public event EventHandler AdClosed;
        public event EventHandler AdOpened;
        #endregion

        #region Ctor

        public InterstitialAd()
        {
            interstitialAd = new Android.Gms.Ads.InterstitialAd(Application.Context);
            interstitialAd.AdListener = this;
            interstitialAd.AdUnitId = GlobalApp.ConfigurationService.GetValue<string>("AdmobAndroidInterstitialProjectAdId");

            if (FeatureRegistry.IsEnabled<IInterstitialAd>())
            {
                LoadAd();
            }
        }
        #endregion

        #region Methods

        public async Task ShowAdAsync()
        {
            // todo: block if ad is loading...
            if (FeatureRegistry.IsEnabled<IInterstitialAd>() && interstitialAd.IsLoaded)
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

            if (FeatureRegistry.IsEnabled<IInterstitialAd>())
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
            var requestbuilder = new AdRequest.Builder();
            requestbuilder.AddTestDevice("5AA4626A9DD6CA8B9D4608A74539FA19");
            interstitialAd.LoadAd(requestbuilder.Build());
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
