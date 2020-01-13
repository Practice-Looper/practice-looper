﻿// Copyright (C)  - All Rights Reserved
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
using Emka3.PracticeLooper.Services.Contracts.Rest;
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
                        Duration = track.Duration
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

        private string SearchTerm { get; set; }
        #endregion

        #region Methods

        private void ExecuteSearchCommandAsync(object o)
        {
            SearchTerm = o as string;
            Console.WriteLine("####### Term " + SearchTerm);

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
                        Console.WriteLine("################### cancelling timer");
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
                    Console.WriteLine("#################### Searching for term: " + SearchTerm);
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

                    Console.WriteLine(res);
                    Console.WriteLine("-------------------------------------------------------------------");

                }, searchCancelTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("################### Search Task Cancelled");
            }
        }

        public override Task InitializeAsync(object parameter)
        {
            spotifyApiService = Factory.GetResolver().Resolve<ISpotifyApiService>();
            sessionsRepository = Factory.GetResolver().Resolve<IRepository<Session>>();
            //spotifyLoader = Factory.GetResolver().Resolve<ISpotifyLoader>();
            //await spotifyLoader.Initialize();

            //spotifyLoader = Factory.GetResolver().Resolve<ISpotifyLoader>();
            //Device.BeginInvokeOnMainThread(()=> { spotifyLoader.Initialize(); });
            return Task.CompletedTask;
        }
        #endregion Metthods
    }
}