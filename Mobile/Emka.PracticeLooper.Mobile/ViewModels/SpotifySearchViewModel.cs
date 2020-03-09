// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.Messenger;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    public class SpotifySearchViewModel : ViewModelBase
    {
        #region Fields
        private ISpotifyApiService spotifyApiService;
        private ISpotifyLoader spotifyLoader;
        private IRepository<Session> sessionsRepository;
        private Command searchCommand;
        private Command createSessionCommand;
        private LooprTimer timer;
        private CancellationTokenSource searchCancelTokenSource;
        #endregion

        #region Ctor
        public SpotifySearchViewModel()
        {
            searchCancelTokenSource = new CancellationTokenSource();
            SearchResults = new ObservableCollection<SpotifyTrack>();
        }
        #endregion

        #region Properties
        public ObservableCollection<SpotifyTrack> SearchResults { get; set; }

        public Command SearchCommand => searchCommand ?? (searchCommand = new Command((o) => ExecuteSearchCommandAsync(o)));

        public Command CreateSessionCommand => createSessionCommand ?? (createSessionCommand = new Command(async o => await ExecuteCreateSessionCommand(o)));
        
        private string SearchTerm { get; set; }
        #endregion

        #region Methods

        private async Task ExecuteCreateSessionCommand(object newTrack)
        {
            var track = newTrack as SpotifyTrack;
            if (track == null)
            {
                throw new ArgumentException(nameof(newTrack));
            }
            try
            {
                var newSession = new Session
                {
                    Name = track.Name,
                    AudioSource = new AudioSource
                    {
                        FileName = track.Id,
                        Type = AudioSourceType.Spotify,
                        Source = track.Uri,
                        Duration = track.Duration / 1000
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

                Device.BeginInvokeOnMainThread(() => MessagingCenter.Send(newSession, MessengerKeys.NewTrackAdded));

                await sessionsRepository.SafeAsync(newSession);

                await NavigationService.GoBackAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw ex;
            }
        }

        private void ExecuteSearchCommandAsync(object o)
        {
            SearchTerm = o as string;

            // emtpy term => remove all items from results
            if (SearchTerm == string.Empty)
            {
                SearchResults.Clear();
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {

                if (timer == null)
                {
                    timer = new LooprTimer(TimeSpan.FromMilliseconds(500),
                        () => { Search(); });
                    timer.Start();
                }
                else
                {
                    if (timer.IsActive)
                    {
                        timer.Stop();
                        if (!searchCancelTokenSource.IsCancellationRequested)
                        {
                            searchCancelTokenSource.Cancel();
                            searchCancelTokenSource = new CancellationTokenSource();
                        }

                        timer.Start();
                    }
                }
            }
        }

        private void Search()
        {
            try
            {
                if (!searchCancelTokenSource.IsCancellationRequested)
                {
                    Task.Run(async () =>
                {

                    var res = await spotifyApiService.SearchTrackByName(SearchTerm, searchCancelTokenSource.Token);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        SearchResults.Clear();
                        foreach (var item in res)
                        {
                            SearchResults.Add(item);
                        }
                    });

                }, searchCancelTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                
            }
        }

        public override async Task InitializeAsync(object parameter)
        {
            Analytics.TrackEvent("[SpotifySearchViewModel] InitializeAsync");
            spotifyApiService = Factory.GetResolver().Resolve<ISpotifyApiService>();
            sessionsRepository = Factory.GetResolver().Resolve<IRepository<Session>>();
            spotifyLoader = Factory.GetResolver().Resolve<ISpotifyLoader>();

            if (!spotifyLoader.Authorized)
            {
                Analytics.TrackEvent("[SpotifySearchViewModel] Initializing Spotify");
                await spotifyLoader.InitializeAsync();
            }
        }
        #endregion Metthods
    }
}