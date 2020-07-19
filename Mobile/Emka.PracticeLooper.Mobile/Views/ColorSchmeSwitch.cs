// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Collections.Generic;
using Emka.PracticeLooper.Mobile.Themes;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Config.Contracts;
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
        #endregion

        #region Ctor

        public ColorSchemeSwitch()
        {
            configurationService = MappingsFactory.Factory.GetResolver().Resolve<IConfigurationService>();
            IsVisible = configurationService.GetValue<bool>(PreferenceKeys.PremiumGeneral);
            configurationService.ValueChanged += ConfigValueChanged;
            Toggled += Switch_Toggled;
            IsToggled = AppInfo.RequestedTheme == AppTheme.Dark || Preferences.Get(PreferenceKeys.ColorScheme, -1) == (int)AppTheme.Dark;
        }
        #endregion

        #region Methods

        void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            mergedDictionaries.Clear();

            if (e.Value)
            {
                mergedDictionaries.Add(new DarkTheme());
                Preferences.Set(PreferenceKeys.ColorScheme, (int)AppTheme.Dark);
                return;
            }

            mergedDictionaries.Add(new LightTheme());
            Preferences.Set(PreferenceKeys.ColorScheme, (int)AppTheme.Light);
        }

        private void ConfigValueChanged(object sender, string e)
        {
            if (e == PreferenceKeys.PremiumGeneral)
            {
                IsVisible = configurationService.GetValue<bool>(PreferenceKeys.PremiumGeneral);
            }
        }
        #endregion
    }
}
