﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Gms.Ads;
using Android.Widget;
using Emka.PracticeLooper.Mobile.Droid.Renderers;
using Emka.PracticeLooper.Mobile.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(AdMobView), typeof(AdMobViewRenderer))]
namespace Emka.PracticeLooper.Mobile.Droid.Renderers
{
    public class AdMobViewRenderer : ViewRenderer<AdMobView, AdView>
    {
        public AdMobViewRenderer(Context context) : base(context)
        {
            
        }

        protected override void OnElementChanged(ElementChangedEventArgs<AdMobView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null && Control == null)
                SetNativeControl(CreateAdView());
            
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(AdView.AdUnitId))
                Control.AdUnitId = Element.AdUnitId;

            
            if (e.PropertyName == "Width")
            {
                SetNativeControl(CreateAdView());
                Element.HeightRequest = GetSmartBannerDpHeight();
            }
        }

        private AdView CreateAdView()
        {
            var adView = new AdView(Context)
            {
                AdSize = AdSize.SmartBanner,
                AdUnitId = Element.AdUnitId
            };

            adView.LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);

            try
            {
                var testDevices = new List<string>
                {
                    AdRequest.DeviceIdEmulator,
                    "F05EA53323F18138CB1722DD1F00A0F0",
                    "B0B92426A54BF08C56BC62D5703161F3",
                    "A808EFF8867E3C5741E6456A4FFAB5D4",
                    "7E7FE7BDD31E1B60AAA1ABA764B55E8B",
                    "A3DC0ED5802A0E68DEC7B1AC713CB73A"
                };

                RequestConfiguration requestConfiguartion
                    = new RequestConfiguration.Builder()
                    .SetTestDeviceIds(testDevices)
                    .Build();

                MobileAds.RequestConfiguration = requestConfiguartion;

                adView.LoadAd(new AdRequest.Builder().Build());
            }
            catch (Exception ex)
            {
                Element?.Logger.LogError(ex);
            }

            return adView;
        }

        private double GetSmartBannerDpHeight() {
            var display = DeviceDisplay.MainDisplayInfo;
            var height = Control.AdSize.GetHeightInPixels(Context)
                / display.Density;
            return height;
        }
    }
}
