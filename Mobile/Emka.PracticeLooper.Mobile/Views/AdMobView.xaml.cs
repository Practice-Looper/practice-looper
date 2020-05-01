// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using Emka.PracticeLooper.Mobile.Common;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Config.Feature;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    public partial class AdMobView : ContentView, IFeature
    {
        public static readonly BindableProperty AdUnitIdProperty = BindableProperty.Create(
                       nameof(AdUnitId),
                       typeof(string),
                       typeof(AdMobView),
                       string.Empty);

        public AdMobView()
        {
            AdUnitId = App.BannerAddUnitId;
            IsVisible = !Factory.GetConfigService().GetValue<bool>(PreferenceKeys.PremiumGeneral);
            Factory.GetConfigService().ValueChanged += AdMobView_ValueChanged;
        }

        private void AdMobView_ValueChanged(object sender, string e)
        {
            if (e == PreferenceKeys.PremiumGeneral)
            {
                IsVisible = !Factory.GetConfigService().GetValue<bool>(PreferenceKeys.PremiumGeneral);
            }
        }

        public string AdUnitId
        {
            get => (string)GetValue(AdUnitIdProperty);
            set => SetValue(AdUnitIdProperty, value);
        }
    }
}
