// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SpotifySearchViewModel : ViewModelBase
    {
        #region Fields
        private ISpotifyApiService spotifyApiService;
        private ISpotifyLoader spotifyLoader;
        private IDialogService dialogService;
        private Command searchCommand;
        private Command createSessionCommand;
        private LooprTimer timer;
        private CancellationTokenSource searchCancelTokenSource;
        private bool isBusy;
        #endregion

        #region Ctor
        public SpotifySearchViewModel(ISpotifyApiService spotifyApiService,
            ISpotifyLoader spotifyLoader,
            INavigationService navigationService,
            ILogger logger,
            IAppTracker appTracker,
            IDialogService dialogService)
            : base(navigationService, logger, appTracker)
        {
            searchCancelTokenSource = new CancellationTokenSource();
            SearchResults = new ObservableCollection<SpotifyTrack>();
            this.spotifyApiService = spotifyApiService ?? throw new ArgumentNullException(nameof(spotifyApiService));
            this.spotifyLoader = spotifyLoader ?? throw new ArgumentNullException(nameof(spotifyLoader));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            UiContext = SynchronizationContext.Current;
        }
        #endregion

        #region Properties
        public ObservableCollection<SpotifyTrack> SearchResults { get; set; }

        public Command SearchCommand => searchCommand ?? (searchCommand = new Command(o => ExecuteSearchCommand(o)));

        public Command CreateSessionCommand => createSessionCommand ?? (createSessionCommand = new Command(async o => await ExecuteCreateSessionCommand(o)));

        private string SearchTerm { get; set; }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Methods

        public async Task ExecuteCreateSessionCommand(object newTrack)
        {
            var track = newTrack as SpotifyTrack;
            if (track == null)
            {
                throw new ArgumentException(nameof(newTrack));
            }
            try
            {
                MessagingCenter.Send(this, MessengerKeys.NewTrackAdded, new AudioSource
                {
                    FileName = track.Name,
                    Type = AudioSourceType.Spotify,
                    Source = track.Uri,
                    Duration = track.Duration / 1000
                });

                await NavigationService.GoBackAsync();
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
                await ShowErrorDialogAsync();
            }
        }

        private void ExecuteSearchCommand(object o)
        {
            try
            {
                IsBusy = true;
                /**/
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
                        timer = new LooprTimer(TimeSpan.FromMilliseconds(1000),
                           async () => await Search());
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
            catch (TaskCanceledException)
            {
                // do nothing
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                ShowErrorDialog();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task Search()
        {
            IsBusy = true;
            try
            {
                if (!searchCancelTokenSource.IsCancellationRequested)
                {
                    var res = await spotifyApiService.SearchTrackByName(SearchTerm, searchCancelTokenSource.Token);
                    UiContext.Send(x =>
                    {
                        SearchResults.Clear();
                        foreach (var item in res)
                        {
                            SearchResults.Add(item);
                        }
                    }, null);
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                ShowErrorDialog();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public override Task InitializeAsync(object parameter)
        {
            //IsBusy = true;
            //try
            //{
            //    if (!spotifyLoader.Authorized)
            //    {
            //        var authorized = await spotifyLoader.InitializeAsync();
            //        if (!authorized)
            //        {
            //            await dialogService.ShowAlertAsync(AppResources.Error_Content_CouldNotConnectToSpotify, AppResources.Error_Caption);
            //            await NavigationService.GoBackAsync();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    await Logger.LogErrorAsync(ex);
            //    await ShowErrorDialogAsync();
            //}
            //finally
            //{
            //    IsBusy = false;
            //}

            return Task.CompletedTask;
        }
        #endregion Metthods
    }
}