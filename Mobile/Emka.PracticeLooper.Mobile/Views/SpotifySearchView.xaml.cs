// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public partial class SpotifySearchView : ContentPage
    {
        public SpotifySearchView()
        {
            InitializeComponent();
            BindingContext = new SpotifySearchViewModel();
        }

        private void OnTrackTapped(object sender, ItemTappedEventArgs e)
        {
            var vm = BindingContext as SpotifySearchViewModel;
            var track = e.Item as SpotifyTrack;
            if (vm != null)
            {
                vm.CreateSessionCommand.Execute(track);
            }
        }

        protected override void OnAppearing()
        {
            SearchBar.Focus();
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
