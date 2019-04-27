// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    public class SpotifySearchViewModel : ViewModelBase
    {
        #region Fields
        private readonly ISpotifyApiService spotifyApiService;
        private readonly ISpotifyLoader spotifyLoader;
        private Command searchCommand;
        private LooprTimer timer;
        private CancellationTokenSource searchCancelTokenSource;
        #endregion

        #region Ctor
        private SpotifySearchViewModel(ISpotifyApiService spotifyApiService, ISpotifyLoader spotifyLoader)
        {
            this.spotifyApiService = spotifyApiService;
            this.spotifyLoader = spotifyLoader;
            searchCancelTokenSource = new CancellationTokenSource();
        }
        #endregion

        #region Properties
        public ObservableCollection<string> SearchResults { get; set; }
        public Command SearchCommand => searchCommand ?? (searchCommand = new Command((o) => ExecuteSearchCommandAsync(o)));
        private string SearchTerm { get; set; }
        #endregion


        #region Methods
        private async Task<SpotifySearchViewModel> InitializeAsync()
        {
            await spotifyLoader.Initialize();
            return this;
        }

        public static Task<SpotifySearchViewModel> CreateAsync(ISpotifyApiService spotifyApiService, ISpotifyLoader spotifyLoader)
        {
            var ret = new SpotifySearchViewModel(spotifyApiService, spotifyLoader);
            return ret.InitializeAsync();
        }

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

                    var res = await spotifyApiService.SearchForTerm(SearchTerm, searchCancelTokenSource.Token);

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
    }

    #endregion
}