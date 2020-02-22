// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using Android.App;
using Android.Gms.Ads;
using Emka3.PracticeLooper.Services.Contracts.Common;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class InterstitialAd : IInterstitialAd
    {
        #region Fields

        Android.Gms.Ads.InterstitialAd interstitialAd;
        #endregion

        #region Ctor

        public InterstitialAd()
        {
            interstitialAd = new Android.Gms.Ads.InterstitialAd(Application.Context);
            interstitialAd.AdUnitId = GlobalApp.ConfigurationService.GetValue<string>("AdmobAndroidInterstitialProjectAdId");
            LoadAd();
        }
        #endregion

        #region Methods

        public void ShowAd()
        {
            if (interstitialAd.IsLoaded)
            {
                interstitialAd.Show();
            }
        }

        void LoadAd()
        {
            var requestbuilder = new AdRequest.Builder();
            interstitialAd.LoadAd(requestbuilder.Build());
        }
        #endregion
    }
}
