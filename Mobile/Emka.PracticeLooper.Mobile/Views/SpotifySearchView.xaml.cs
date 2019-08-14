// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Messenger;
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
        }

        private async void OnTrackTapped(object sender, ItemTappedEventArgs e)
        {
            var vm = BindingContext as SpotifySearchViewModel;
            var track = e.Item as SpotifyTrack;
            if (vm != null)
            {
                var session = CreateSession(track);
                Device.BeginInvokeOnMainThread(() => MessagingCenter.Send(session, MessengerKeys.NewTrackAdded));
            }

            await Navigation.PopAsync();
        }

        public Session CreateSession(SpotifyTrack track)
        {

            try
            {
                var newSession = new Session
                {
                    Name = track.Name,
                    AudioSource = new AudioSource
                    {
                        FileName = track.Id,
                        Type = AudioSourceType.Spotify,
                        Source = track.Uri
                    },
                    Loops = new List<Loop>
                            {
                                new Loop
                                {
                                    Name = track.Name,
                                    StartPosition = 0.0,
                                    EndPosition = 1.0,
                                    Repititions = 0
                                }
                            }
                };


                return newSession;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw ex;
            }
        }
    }
}
