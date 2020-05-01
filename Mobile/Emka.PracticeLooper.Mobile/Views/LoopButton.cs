// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Config.Feature;
using Emka3.PracticeLooper.Utils;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public class LoopButton : ImageButton, IFeature
    {
        public LoopButton()
        {
            IsVisible = Factory.GetConfigService().GetValue<bool>(PreferenceKeys.PremiumGeneral);
            Factory.GetConfigService().ValueChanged += ConfigValueChanged;
        }

        private void ConfigValueChanged(object sender, string e)
        {
            if (e == PreferenceKeys.PremiumGeneral)
            {
                IsVisible = Factory.GetConfigService().GetValue<bool>(PreferenceKeys.PremiumGeneral);
            }
        }
    }
}
