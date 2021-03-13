// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using Emka.PracticeLooper.Mobile.Themes;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Config.Contracts.Features;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public class ColorSchemeSwitch : Switch
    {
        #region Fields
        private readonly IConfigurationService configurationService;
        private readonly IFeatureRegistry featureRegistry;
        #endregion

        #region Ctor

        public ColorSchemeSwitch()
        {
            var resolver = MappingsFactory.Factory.GetResolver() ?? throw new ArgumentNullException("resolver");
            configurationService = resolver.Resolve<IConfigurationService>() ?? throw new ArgumentNullException(nameof(configurationService));
            featureRegistry = resolver.Resolve<IFeatureRegistry>() ?? throw new ArgumentNullException(nameof(featureRegistry));
            featureRegistry.RegisterForUpdates<PremiumFeature>(PremiumFeatureToggle);
            IsVisible = featureRegistry.IsEnabled<PremiumFeature>();
            Toggled += Switch_Toggled;
            IsToggled = configurationService.GetValue(PreferenceKeys.ColorScheme, -1) == (int)AppTheme.Dark;
        }
        #endregion

        #region Methods
        void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;

            if (e.Value && configurationService.GetValue(PreferenceKeys.ColorScheme, -1) != (int)AppTheme.Dark)
            {
                mergedDictionaries.Clear();
                mergedDictionaries.Add(new DarkTheme());
                configurationService.SetValue(PreferenceKeys.ColorScheme, (int)AppTheme.Dark, true);
                return;
            }
            else if(!e.Value && configurationService.GetValue(PreferenceKeys.ColorScheme, -1) != (int)AppTheme.Light)
            {
                mergedDictionaries.Clear();
                mergedDictionaries.Add(new LightTheme());
                configurationService.SetValue(PreferenceKeys.ColorScheme, (int)AppTheme.Light, true);
            }
        }

        public void PremiumFeatureToggle(bool enabled)
        {
            IsVisible = featureRegistry.IsEnabled<PremiumFeature>();
        }
        #endregion
    }
}
