// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Model.Player;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
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
    }
}
