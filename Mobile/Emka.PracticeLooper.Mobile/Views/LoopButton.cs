// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Utils;
using Xamarin.Forms;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public class LoopButton : ImageButton, IFeature
    {
        #region Fields

        private readonly IConfigurationService configurationService;
        #endregion

        #region Ctor

        public LoopButton()
        {
            configurationService = MappingsFactory.Factory.GetResolver().Resolve<IConfigurationService>();
            IsVisible = configurationService.GetSecureValue<bool>(PreferenceKeys.PremiumGeneral);
            configurationService.ValueChanged += ConfigValueChanged;
        }
        #endregion

        #region Methods

        private void ConfigValueChanged(object sender, string e)
        {
            if (e == PreferenceKeys.PremiumGeneral)
            {
                IsVisible = configurationService.GetSecureValue<bool>(PreferenceKeys.PremiumGeneral);
            }
        }
        #endregion
    }
}
