// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.ComponentModel;
using Emka.PracticeLooper.Mobile.iOS.Renderer;
using Emka.PracticeLooper.Mobile.Views;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Config.Contracts.Features;
using Google.MobileAds;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

[assembly: ExportRenderer(typeof(AdMobView), typeof(AdMobViewRenderer))]
namespace Emka.PracticeLooper.Mobile.iOS.Renderer
{
    public class AdMobViewRenderer : ViewRenderer<AdMobView, BannerView>
    {
        #region Fields
        private readonly IConfigurationService configurationService;
        private readonly IFeatureRegistry featureRegistry;
        #endregion

        #region Ctor

        public AdMobViewRenderer()
        {
            var resolver = MappingsFactory.Factory.GetResolver() ?? throw new ArgumentNullException("resolver");
            configurationService = resolver.Resolve<IConfigurationService>();
            featureRegistry = resolver.Resolve<IFeatureRegistry>() ?? throw new ArgumentNullException(nameof(featureRegistry));
        }
        #endregion

        #region Methods

        protected override void OnElementChanged(ElementChangedEventArgs<AdMobView> e)
        {
            base.OnElementChanged(e);
            if (!featureRegistry.IsEnabled<PremiumFeature>() && Control == null)
            {
                SetNativeControl(CreateBannerView());
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(BannerView.AdUnitId))
                Control.AdUnitId = Element.AdUnitId;

            if (e.PropertyName == "Width")
            {
                Element.HeightRequest = GetSmartBannerDpHeight();
            }
        }

        private BannerView CreateBannerView()
        {
            var bannerView = new BannerView(AdSizeCons.SmartBannerPortrait)
            {
                AdUnitId = Element.AdUnitId,
                RootViewController = GetVisibleViewController()
            };

            bannerView.LoadRequest(GetRequest());

            Request GetRequest()
            {
                var request = Request.GetDefaultRequest();
                MobileAds.SharedInstance.RequestConfiguration.TestDeviceIdentifiers = new[] { "6fb304bbcc401debac41d2255509463f", "3408f4ee8f8d77f2efcbf255ead140d6", "4110822911a7f735dfffac62fcceea46" };
                return request;
            }

            return bannerView;
        }

        private UIViewController GetVisibleViewController()
        {
            var windows = UIApplication.SharedApplication.Windows;
            foreach (var window in windows)
            {
                if (window.RootViewController != null)
                {
                    return window.RootViewController;
                }
            }
            return null;
        }

        private double GetSmartBannerDpHeight()
        {
            var display = DeviceDisplay.MainDisplayInfo;
            var dpHeight = display.Height / display.Density;

            if (dpHeight <= 400) return 32;
            if (dpHeight > 400 && dpHeight <= 720) return 50;
            return 90;
        }
        #endregion
    }
}
