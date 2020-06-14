// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.ComponentModel;
using Android.Content;
using Android.Gms.Ads;
using Android.Widget;
using Emka.PracticeLooper.Mobile.Droid.Renderers;
using Emka.PracticeLooper.Mobile.Views;
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
                AdRequest request;
#if DEBUG
                request = new AdRequest.Builder().AddTestDevice("7E7FE7BDD31E1B60AAA1ABA764B55E8B").Build();
#else
                request = new AdRequest.Builder().Build();
#endif
                adView.LoadAd(request);
            }
            catch (Exception ex)
            {
                Element?.Logger.LogError(ex);
            }

            return adView;
        }
    }
}
