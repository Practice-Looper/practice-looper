// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public partial class LoopsDetailsView : ContentPage
    {
        public LoopsDetailsView()
        {
            InitializeComponent();
            var resolver = Emka3.PracticeLooper.Mappings.Factory.GetResolver();
            BindingContext = new SessionDetailsViewModel(resolver.Resolve<IConfigurationService>());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                var safeInsets = On<iOS>().SafeAreaInsets();
                safeInsets.Left = safeInsets.Right = 10;
                Padding = safeInsets;
            }
        }
    }
}
