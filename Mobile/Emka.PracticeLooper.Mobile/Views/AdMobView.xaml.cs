// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Config.Contracts.Features;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    public partial class AdMobView : ContentView
    {
        public static readonly BindableProperty AdUnitIdProperty = BindableProperty.Create(
                       nameof(AdUnitId),
                       typeof(string),
                       typeof(AdMobView),
                       string.Empty);
        private readonly IFeatureRegistry featureRegistry;
        private readonly IConfigurationService configurationService;

        public AdMobView()
        {
            var resolver = Emka3.PracticeLooper.Mappings.Factory.GetResolver() ?? throw new ArgumentNullException("resolver");
            featureRegistry = resolver.Resolve<IFeatureRegistry>() ?? throw new ArgumentNullException("featureRegistry");
            featureRegistry.RegisterForUpdates<PremiumFeature>(Toggle);
            configurationService = resolver.Resolve<IConfigurationService>() ?? throw new NullReferenceException("configurationService");
            Logger = Emka3.PracticeLooper.Mappings.Factory.GetResolver().Resolve<ILogger>() ?? throw new ArgumentNullException("Logger");
            AdUnitId = configurationService.GetValue(Device.RuntimePlatform == Device.iOS ? "AdmobIosTopBannerAdId" : "AdmobAndroidTopBannerAdId");
            IsVisible = !featureRegistry.IsEnabled<PremiumFeature>();
        }

        public void Toggle(bool enabled)
        {
            IsVisible = !featureRegistry.IsEnabled<PremiumFeature>();
        }

        public string AdUnitId
        {
            get => (string)GetValue(AdUnitIdProperty);
            set => SetValue(AdUnitIdProperty, value);
        }

        public ILogger Logger { get; }
    }
}
