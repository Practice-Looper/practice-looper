// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Forms;
using Emka3.PracticeLooper.Utils;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka3.PracticeLooper.Config.Contracts;
using System.IO;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using System.Net;
using System.Threading;
using Emka3.PracticeLooper.Model.Common;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private IRepository<Session> sessionsRepository;
        private IRepository<Loop> loopsRepository;
        private IDialogService dialogService;
        private IFileRepository fileRepository;
        private ISourcePicker sourcePicker;
        private ISpotifyLoader spotifyLoader;
        private ISpotifyApiService spotifyApiService;
        private Mobile.Common.IFilePicker filePicker;
        private readonly IConnectivityService connectivityService;
        private readonly IConfigurationService configurationService;
        private readonly IReviewRequestService reviewRequestService;
        private Command playCommand;
        private Command createSessionCommand;
        private Command deleteSessionCommand;
        private Command pickSourceCommand;
        private Command navigateToSettingsCommand;
        private Command toggleSpotifyWebPlayerCommand;
        private Command spotifyWebPlayeRefreshCommand;
        private Command spotifyWebPlayerGoBackCommand;
        private Command spotifyWebPlayerGoForwardCommand;
        private SessionViewModel currentSession;
        private Loop currentLoop;
        private bool isPlaying;
        private bool showCallToAction;
        private double minimumValue;
        private double maximumValue;
        private string songDuration;
        private string currentSongTime;
        private Command addNewLoopCommand;
        private bool isBusy;
        private bool isPremiumUser;
        private double stepFrequency;
        private bool isSpotifyWebPlayerVisible;
        private TaskCompletionSource<bool> spotifyWebPlayerActivationTokenSource;
        private TaskCompletionSource<bool> spotifyWebPlayerLoadedTokenSource;
        private bool spotifyWebPlayerHasBeenLoaded;
        private bool spotifyWebPlayerHasBeenActivated;
        private string spotifyDeviceId;
        private readonly CurrentPlatform currentPlatform;
        #endregion

        #region Ctor
        public MainViewModel(
            IRepository<Session> sessionsRepository,
            IRepository<Loop> loopsRepository,
            IDialogService dialogService,
            IFileRepository fileRepository,
            ISourcePicker sourcePicker,
            ISpotifyLoader spotifyLoader,
            ISpotifyApiService spotifyApiService,
            Mobile.Common.IFilePicker filePicker,
            IConnectivityService connectivityService,
            INavigationService navigationService,
            ILogger logger,
            IAppTracker appTracker,
            IConfigurationService configurationService,
            IEnumerable<IAudioPlayer> audioPlayers,
            IReviewRequestService reviewRequestService)
            : base(navigationService, logger, appTracker)
        {
            this.sessionsRepository = sessionsRepository ?? throw new ArgumentNullException(nameof(sessionsRepository));
            this.loopsRepository = loopsRepository ?? throw new ArgumentNullException(nameof(loopsRepository));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            this.sourcePicker = sourcePicker ?? throw new ArgumentNullException(nameof(sourcePicker));
            this.spotifyLoader = spotifyLoader ?? throw new ArgumentNullException(nameof(spotifyLoader));
            this.spotifyApiService = spotifyApiService ?? throw new ArgumentNullException(nameof(spotifyApiService));
            this.filePicker = filePicker ?? throw new ArgumentNullException(nameof(filePicker));
            this.connectivityService = connectivityService ?? throw new ArgumentNullException(nameof(connectivityService));
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            AudioPlayers = audioPlayers?.ToList() ?? throw new ArgumentNullException(nameof(audioPlayers));

            MessagingCenter.Subscribe<object, AudioSource>(this, MessengerKeys.NewTrackAdded, (sender, audioSorce) => CreateSessionCommand.Execute(audioSorce));
            MessagingCenter.Subscribe<SessionViewModel, SessionViewModel>(this, MessengerKeys.DeleteSession, (sender, arg) => DeleteSessionCommand.Execute(arg));
            MessagingCenter.Subscribe<SessionDetailsViewModel, LoopViewModel>(this, MessengerKeys.LoopChanged, OnLoopChanged);
            MessagingCenter.Subscribe<LoopViewModel, Loop>(this, MessengerKeys.DeleteLoop, OnDeleteLoop);
            MessagingCenter.Subscribe<object, bool>(this, MessengerKeys.SpotifyWebPlayerLoaded, OnSpotifyWebPlayerPlayerLoaded);
            MessagingCenter.Subscribe<object, bool>(this, MessengerKeys.SpotifyPlayerActivated, OnSpotifyWebPlayerActivated);
            MessagingCenter.Subscribe<object>(this, MessengerKeys.WebViewRefreshInitialized, OnWebViewRefreshInitialized);
            MessagingCenter.Subscribe<object, Session>(this, MessengerKeys.UpdateSession, async (sender, arg) => await OnUpdateSession(arg));
            Sessions = new ObservableCollection<SessionViewModel>();
            isPlaying = false;
            currentPlatform = configurationService.GetValue<CurrentPlatform>("Platform");
            showCallToAction = configurationService.GetValue<bool>("IsFirstLaunchEver") && currentPlatform == CurrentPlatform.iOS;
            UiContext = SynchronizationContext.Current;
        }
        #endregion

        #region Properties
        public Command DeleteSessionCommand => deleteSessionCommand ??= new Command(async (o) => await ExecuteDeleteSessionCommandAsync(o));
        public Command PlayCommand => playCommand ??= new Command(async (o) => await ExecutePlayCommand(o), CanExecutePlayCommand);
        public Command CreateSessionCommand => createSessionCommand ??= new Command(async (o) => await ExecuteCreateSessionCommandAsync(o), CanExecuteCreateSessionCommand);
        public Command PickSourceCommand => pickSourceCommand ??= new Command(async (o) => await ExecutePickSourceCommandAsync(o));
        public Command AddNewLoopCommand => addNewLoopCommand ??= new Command(async (o) => await ExecuteAddNewLoopCommand(o), CanExecuteAddNewLoopCommand);
        public Command NavigateToSettingsCommand => navigateToSettingsCommand ??= new Command(async () => await ExecuteNavigateToSettingsCommand());
        public Command ToggleSpotifyWebPlayerCommand => toggleSpotifyWebPlayerCommand ??= new Command(ExecuteToggleSpotifyWebPlayerCommand);
        public Command SpotifyWebPlayeRefreshCommand => spotifyWebPlayeRefreshCommand ??= new Command(ExecuteSpotifyWebPlayeRefreshCommand);
        public Command SpotifyWebPlayerGoBackCommand => spotifyWebPlayerGoBackCommand ??= new Command(ExecuteSpotifyWebPlayerGoBackCommand);
        public Command SpotifyWebPlayerGoForwardCommand => spotifyWebPlayerGoForwardCommand ??= new Command(ExecuteSpotifyWebPlayerGoForwardCommand);

        public IAudioPlayer CurrentAudioPlayer { get; private set; }

        public bool IsSpotifyWebPlayerVisible
        {
            get => isSpotifyWebPlayerVisible;
            set
            {
                isSpotifyWebPlayerVisible = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                if (isPlaying != value)
                {
                    isPlaying = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Minimum
        {
            get => 0;
        }

        public double MinimumValue
        {
            get => minimumValue;
            set
            {
                minimumValue = value < Minimum ? Minimum : value;
                NotifyPropertyChanged(nameof(LoopStartPosition));
            }
        }

        public double Maximum
        {
            get => 1;
        }

        public double MaximumValue
        {
            get => maximumValue;
            set
            {
                maximumValue = value > Maximum ? Maximum : value;
                NotifyPropertyChanged(nameof(LoopEndPosition));
            }
        }

        public string SongDuration
        {
            get => songDuration;
            set
            {
                songDuration = value;
                NotifyPropertyChanged();
            }
        }

        public string CurrentSongTime
        {
            get => currentSongTime;
            set
            {
                currentSongTime = value;
                NotifyPropertyChanged();
            }
        }

        public string LoopStartPosition
        {
            get => CurrentLoop != null && CurrentLoop.Session != null ? FormatTime(minimumValue * CurrentLoop.Session.AudioSource.Duration * 1000) : string.Empty;
        }

        public string LoopEndPosition
        {
            get => CurrentLoop != null ? FormatTime(maximumValue * CurrentLoop.Session.AudioSource.Duration * 1000) : string.Empty;
        }

        public double MinimumRange { get => StepFrequency * 5; }

        public bool IsInitialized => CurrentSession != null;

        public ObservableCollection<SessionViewModel> Sessions { get; set; }

        public bool ShowCallToAction
        {
            get { return showCallToAction; }
            set
            {
                if (showCallToAction != value)
                {
                    showCallToAction = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public SessionViewModel CurrentSession
        {
            get
            {
                return currentSession;
            }

            set
            {
                currentSession = value;
                var isCurrentlyPlaying = IsPlaying;
                if (currentSession != null)
                {
                    StepFrequency = currentSession != null ? 1 / currentSession.Session.AudioSource.Duration : 0;
                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "CurrentSession", $"StepFrequency {StepFrequency}" } });

                    LoadAudioPlayer();
                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "CurrentSession", $"Audio player loaded" } });

                    if (configurationService.GetValue(PreferenceKeys.LastSession, default(int)) != currentSession.Session.Id)
                    {
                        configurationService.SetValue(PreferenceKeys.LastSession, currentSession.Session.Id, true);
                        Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "CurrentSession", $"LastSession updated" } });
                    }

                    if (isCurrentlyPlaying)
                    {
                        PlayCommand.Execute(null);
                    }

                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "CurrentSession", "isCurrentlyPlaying handled" } });
                }

                NotifyPropertyChanged();
                Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "CurrentSession", "NotifyPropertyChanged" } });

                NotifyPropertyChanged(nameof(IsInitialized));
                Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "CurrentSession", "NotifyPropertyChanged IsInitialized" } });

                NotifyPropertyChanged(nameof(IsPlaying));
                Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "CurrentSession", "NotifyPropertyChanged IsPlaying" } });

                NotifyPropertyChanged(nameof(MinimumRange));
                Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "CurrentSession", "NotifyPropertyChanged MinimumRange" } });

                NotifyPropertyChanged(nameof(MinimumValue));
                Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "CurrentSession", "NotifyPropertyChanged MinimumValue" } });

                NotifyPropertyChanged(nameof(MaximumValue));
                Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "CurrentSession", "NotifyPropertyChanged MaximumValue" } });

                PlayCommand.ChangeCanExecute();
                Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "CurrentSession", "PlayCommand.ChangeCanExecute" } });

                AddNewLoopCommand.ChangeCanExecute();
                Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "CurrentSession", "AddNewLoopCommand.ChangeCanExecute(" } });
            }
        }

        public Loop CurrentLoop
        {
            get
            {
                return currentLoop;
            }

            set
            {
                currentLoop = value;
                if (currentLoop != null)
                {
                    if (configurationService.GetValue(PreferenceKeys.LastLoop, default(int)) != currentLoop.Id)
                    {
                        configurationService.SetValue(PreferenceKeys.LastLoop, currentLoop.Id, true);
                    }
                }

                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(MinimumValue));
                NotifyPropertyChanged(nameof(MaximumValue));
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsPremiumUser
        {
            get => isPremiumUser;
            set
            {
                isPremiumUser = value;
                NotifyPropertyChanged();
            }
        }

        public double StepFrequency
        {
            get => stepFrequency;
            set
            {
                stepFrequency = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(MinimumRange));
            }
        }

        public List<IAudioPlayer> AudioPlayers { get; }
        #endregion

        #region Methods
        public override async Task InitializeAsync(object parameter)
        {
            await Tracker.TrackAsync(TrackerEvents.Init, new Dictionary<string, string> { { "InitializeAsync", "Enter" } });
            if (UiContext == null)
            {
                throw new InvalidOperationException("UiContext is null!");
            }

            IsBusy = true;
            try
            {
                await sessionsRepository.InitAsync();
                await Tracker.TrackAsync(TrackerEvents.Init, new Dictionary<string, string> { { "InitializeAsync", "sessionsRepository initialized" } });

                await loopsRepository.InitAsync();
                await Tracker.TrackAsync(TrackerEvents.Init, new Dictionary<string, string> { { "InitializeAsync", "loopsRepository initialized" } });

                var items = await sessionsRepository.GetAllItemsAsync().ConfigureAwait(false);
                items = items?.OrderByDescending(i => i.AudioSource.Type).ToList();
                await Tracker.TrackAsync(TrackerEvents.Init, new Dictionary<string, string> { { "InitializeAsync", "sessionsRepository items lodaded" } });

                spotifyLoader.Disconnected += OnSpotifyDisconnected;
                await Tracker.TrackAsync(TrackerEvents.Init, new Dictionary<string, string> { { "InitializeAsync", "spotifyLoader disconnected event attached" } });

                if (items != null && items.Any())
                {
                    await Tracker.TrackAsync(TrackerEvents.Init, new Dictionary<string, string> { { "InitializeAsync", $"loaded {items.Count} items" } });
                    UiContext?.Send(x =>
                            {
                                try
                                {
                                    foreach (var item in items)
                                    {
                                        var newViewModel = new SessionViewModel(item, dialogService, Logger, NavigationService, Tracker);
                                        Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "InitializeAsync", $"view model status {newViewModel}" } });
                                        Sessions.Add(newViewModel);
                                    }

                                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "InitializeAsync", "sessions added" } });

                                    var lastUsedSessionId = configurationService.GetValue(PreferenceKeys.LastSession, default(int));
                                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "InitializeAsync", $"lastUsedSessionId {lastUsedSessionId}" } });

                                    var lastUsedSession = Sessions.FirstOrDefault(s => s.Session.Id == lastUsedSessionId) ?? Sessions.FirstOrDefault();
                                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "InitializeAsync", $"lastUsedSession {lastUsedSession}" } });

                                    CurrentSession = lastUsedSession;
                                }
                                catch (Exception ex)
                                {
                                    Logger?.LogError(ex);
                                }
                            }, null);
                }

                if (!connectivityService.HasFastConnection() && !configurationService.GetValue(PreferenceKeys.SlowConnectionWarning, default(bool)))
                {
                    await dialogService.ShowAlertAsync(AppResources.Hint_Content_SlowConnection, AppResources.Hint_Caption_SlowConnection);
                    await configurationService.SetValueAsync(PreferenceKeys.SlowConnectionWarning, true, true);
                }

                if (reviewRequestService != null)
                {
                    await reviewRequestService.RequestReview();
                }

                await FetchSpotifyCovers().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Logger?.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
            }
            finally
            {
                IsBusy = false;
                Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "InitializeAsync", "InitializeAsync exit" } });
            }
        }

        private void OnSpotifyDisconnected(object sender, EventArgs args)
        {
            if (CurrentAudioPlayer != null && CurrentAudioPlayer.Types.HasFlag(AudioSourceType.Spotify))
            {
                CurrentAudioPlayer?.Pause();
                CurrentAudioPlayer.PlayStatusChanged -= OnPlayingStatusChanged;
                CurrentAudioPlayer.CurrentTimePositionChanged -= OnCurrentTimePositionChanged;
            }
        }

        public void Pause()
        {
            try
            {
                if (CurrentAudioPlayer != null)
                {
                    CurrentAudioPlayer.Pause();
                    CurrentAudioPlayer.PlayStatusChanged -= OnPlayingStatusChanged;
                    CurrentAudioPlayer.CurrentTimePositionChanged -= OnCurrentTimePositionChanged;
                }

                spotifyLoader?.Disconnect();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
            }
        }

        private void ExecuteSpotifyWebPlayerGoBackCommand(object o)
        {
            MessagingCenter.Send<object>(this, MessengerKeys.WebViewGoBackToggled);
        }

        private void ExecuteSpotifyWebPlayerGoForwardCommand(object o)
        {
            MessagingCenter.Send<object>(this, MessengerKeys.WebViewGoForwardToggled);
        }

        private void ExecuteSpotifyWebPlayeRefreshCommand(object o)
        {
            MessagingCenter.Send<object>(this, MessengerKeys.WebViewRefreshInitialized);
        }

        private bool CanExecuteCreateSessionCommand(object arg)
        {
            return true;
        }

        public async Task ExecuteCreateSessionCommandAsync(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            var source = o as AudioSource;

            if (source != null)
            {
                try
                {
                    var newSession = new Session
                    {
                        Name = source.FileName,
                        Artist = source.Artist,
                        CoverSource = source.CoverSource,
                        AudioSource = source,
                        Loops = new List<Loop>
                        {
                            new Loop
                            {
                                Name = source.FileName,
                                StartPosition = 0.0,
                                EndPosition = 1.0,
                                Repititions = 0,
                                IsDefault = true
                            }
                        }
                    };

                    var newSessionViewModel = new SessionViewModel(newSession, dialogService, Logger, NavigationService, Tracker);
                    var isCurrentlyPlayling = CurrentAudioPlayer != null && CurrentAudioPlayer.IsPlaying;

                    if (isCurrentlyPlayling)
                    {
                        CurrentAudioPlayer?.Pause();
                    }

                    newSession.Id = await sessionsRepository.SaveAsync(newSession);
                    Sessions.Add(newSessionViewModel);
                    CurrentSession = newSessionViewModel;

                    if (isCurrentlyPlayling)
                    {
                        PlayCommand.Execute(default);
                    }
                }
                catch (Exception ex)
                {
                    await Logger.LogErrorAsync(ex);
                    await dialogService.ShowAlertAsync(AppResources.Error_Content_CouldNotCreateSession, AppResources.Error_Caption);
                }
            }
        }

        private bool CanExecutePlayCommand(object o)
        {
            return CurrentSession != null && CurrentAudioPlayer != null;
        }

        public async Task ExecutePlayCommand(object o)
        {
            try
            {
                IsBusy = true;

                if (CurrentAudioPlayer != null && CurrentAudioPlayer.IsPlaying)
                {
                    UiContext.Send(x => IsPlaying = false, null);
                    UiContext.Send(x => CurrentAudioPlayer.Pause(true), null);
                    CurrentAudioPlayer.PlayStatusChanged -= OnPlayingStatusChanged;
                    CurrentAudioPlayer.CurrentTimePositionChanged -= OnCurrentTimePositionChanged;
                }
                else
                {
                    var initResult = false;
                    CurrentAudioPlayer.PlayStatusChanged += OnPlayingStatusChanged;
                    CurrentAudioPlayer.CurrentTimePositionChanged += OnCurrentTimePositionChanged;

                    if (CurrentAudioPlayer.Types.HasFlag(AudioSourceType.Spotify))
                    {
                        initResult = await HandleSpotifyPlayerInitialization();
                    }

                    if (CurrentAudioPlayer.Types.HasFlag(AudioSourceType.LocalInternal | AudioSourceType.LocalExternal))
                    {
                        initResult = await InitAudioPlayer(false);
                    }

                    if (CurrentAudioPlayer.Initialized && initResult)
                    {
                        await CurrentAudioPlayer.PlayAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync(AppResources.Error_Content_CouldNotPlaySong, AppResources.Error_Caption);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteDeleteSessionCommandAsync(object session)
        {
            if (session is SessionViewModel sessionToDelete)
            {
                try
                {
                    if (IsPlaying && CurrentSession == sessionToDelete)
                    {
                        CurrentAudioPlayer.Pause();
                        IsPlaying = false;
                    }

                    await sessionsRepository.DeleteAsync(sessionToDelete.Session);

                    if (sessionToDelete.Session.AudioSource.Type == AudioSourceType.LocalInternal)
                    {
                        await fileRepository.DeleteFileAsync(sessionToDelete.Session.AudioSource.Source);
                    }

                    UiContext.Send(x =>
                    {
                        Sessions.Remove(sessionToDelete);
                        if (Sessions.Any() && CurrentSession != null)
                        {
                            CurrentSession = Sessions.First();
                            CurrentLoop = currentSession.Session.Loops.First();
                        }
                        else
                        {
                            if (configurationService.GetValue(PreferenceKeys.LastLoop, -1) == CurrentLoop.Id)
                            {
                                configurationService.ClearValue(PreferenceKeys.LastLoop);
                            }

                            CurrentLoop = null;

                            if (configurationService.GetValue(PreferenceKeys.LastSession, -1) == CurrentSession.Session.Id)
                            {
                                configurationService.ClearValue(PreferenceKeys.LastSession);
                            }

                            CurrentSession = null;
                        }

                        NotifyPropertyChanged(nameof(IsInitialized));
                    }, null);
                }
                catch (Exception ex)
                {
                    await Logger.LogErrorAsync(ex);
                    await dialogService.ShowAlertAsync(AppResources.Error_Content_CouldNotDeleteSong, AppResources.Error_Caption);
                }
            }
            else
            {
                throw new ArgumentException(nameof(session));
            }
        }

        private async Task ExecutePickSourceCommandAsync(object o)
        {
            ShowCallToAction = false;

            try
            {
                UiContext.Send(x => IsBusy = true, null);
                var source = await sourcePicker?.SelectFileSource();

                switch (source)
                {
                    case AudioSourceType.LocalInternal:

                        try
                        {
                            var newFile = await filePicker.ShowPicker();
                            // file is null when user cancelled file picker!
                            if (newFile != null)
                            {
                                CreateSessionCommand.Execute(newFile);
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            await Logger.LogErrorAsync(ex);
                            await dialogService.ShowAlertAsync(AppResources.Error_Content_NotEnoughSpace, AppResources.Error_Caption);
                        }

                        break;
                    case AudioSourceType.Spotify:

                        if (currentPlatform == CurrentPlatform.Droid && !spotifyLoader.IsSpotifyInstalled())
                        {
                            await InitiateSpotifyInstallationAsync();
                            return;
                        }

                        // spotify already authorized?
                        if (spotifyLoader != null && !spotifyLoader.Authorized)
                        {
                            await spotifyLoader.InitializeAsync();
                        }

                        // something went wrong and authorization failed
                        if (!spotifyLoader.Authorized)
                        {
                            await dialogService.ShowAlertAsync(AppResources.Error_Content_CouldNotConnectToSpotify, AppResources.Error_Caption);
                            return;
                        }

                        // everything fine, go to search view
                        if (spotifyLoader.Authorized)
                        {
                            await NavigationService.NavigateToAsync<SpotifySearchViewModel>();
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
            }
            finally
            {
                UiContext.Send(x => IsBusy = false, null);
            }
        }

        public bool CanExecuteAddNewLoopCommand(object arg)
        {
            return CurrentSession != null;
        }

        public async Task ExecuteAddNewLoopCommand(object o)
        {
            IsBusy = true;
            try
            {
                var name = await dialogService.ShowPromptAsync(
                    AppResources.Prompt_Caption_NewLoop,
                    AppResources.Prompt_Content_NewLoop,
                    AppResources.Save,
                    AppResources.Cancel,
                    string.Format(AppResources.Prompt_Content_NewLoop_NamePlaceholder, CurrentSession.Session.Loops.Count));

                if (name != null)
                {
                    if (name == string.Empty)
                    {
                        name = string.Format(AppResources.Prompt_Content_NewLoop_NamePlaceholder, CurrentSession.Session.Loops.Count);
                    }

                    // reset current loop
                    var unmodifiedLoop = await loopsRepository.GetByIdAsync(CurrentLoop.Id);
                    CurrentSession.Session.Loops[CurrentSession.Session.Loops.IndexOf(CurrentLoop)] = unmodifiedLoop;

                    // create new loop and set it as current loop
                    var loop = new Loop() { Name = name, StartPosition = MinimumValue, EndPosition = MaximumValue, Session = CurrentSession.Session, SessionId = CurrentSession.Session.Id, IsDefault = false };
                    loop.Id = await loopsRepository.SaveAsync(loop);
                    CurrentSession.Session.Loops.Add(loop);
                    CurrentLoop = loop;
                }

            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync(AppResources.Error_Content_CouldNotCreateLoop, AppResources.Error_Caption);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void LoadAudioPlayer()
        {
            try
            {
                Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "LoadAudioPlayer", "entered" } });
                IsBusy = true;

                if (IsPlaying)
                {
                    CurrentAudioPlayer?.Pause(false);
                }

                if (CurrentSession != null)
                {
                    CurrentAudioPlayer = AudioPlayers.First(p => p.Types.HasFlag(CurrentSession.Session.AudioSource.Type));
                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "LoadAudioPlayer", $"CurrentAudioPlayer {CurrentAudioPlayer != null}" } });

                    var lastUsedLoopId = configurationService.GetValue(PreferenceKeys.LastLoop, default(int));
                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "LoadAudioPlayer", $"lastUsedLoopId {lastUsedLoopId}" } });

                    CurrentLoop = CurrentSession.Session.Loops.FirstOrDefault(l => l.Id == lastUsedLoopId) ?? CurrentSession.Session.Loops.First(l => l.IsDefault);
                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "LoadAudioPlayer", $"CurrentLoop {CurrentLoop}" } });

                    MinimumValue = CurrentLoop.StartPosition;
                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "LoadAudioPlayer", $"MinimumValue {MinimumValue}" } });

                    MaximumValue = CurrentLoop.EndPosition;
                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "LoadAudioPlayer", $"MaximumValue {MaximumValue}" } });

                    SongDuration = FormatTime(CurrentSession.Session.AudioSource.Duration * 1000);
                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "LoadAudioPlayer", $"SongDuration {SongDuration}" } });

                    CurrentSongTime = FormatTime(CurrentSession.Session.AudioSource.Duration * 1000 * MinimumValue);
                    Tracker.Track(TrackerEvents.Init, new Dictionary<string, string> { { "LoadAudioPlayer", $"CurrentSongTime {CurrentSongTime}" } });
                }
                else
                {
                    MinimumValue = 0;
                    MaximumValue = 1;
                    SongDuration = FormatTime(0.0);
                    CurrentSongTime = FormatTime(0.0);

                    if (CurrentAudioPlayer != null)
                    {
                        CurrentAudioPlayer.PlayStatusChanged -= OnPlayingStatusChanged;
                        CurrentAudioPlayer.CurrentTimePositionChanged -= OnCurrentTimePositionChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorAsync(ex);
                dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<bool> InitAudioPlayer(bool useWebPlayer, string deviceId = null)
        {
            if (!CurrentAudioPlayer.Initialized)
            {
                try
                {
                    await CurrentAudioPlayer.InitAsync(CurrentLoop, useWebPlayer, deviceId);
                }
                catch (FileNotFoundException ex)
                {
                    await Logger.LogErrorAsync(ex);
                    var deleteFile = await dialogService.ShowConfirmAsync(AppResources.Error_Caption, AppResources.Error_Content_FileNotFound, AppResources.Cancel, AppResources.Ok);

                    if (deleteFile)
                    {
                        DeleteSessionCommand.Execute(CurrentSession);
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    await Logger.LogErrorAsync(ex);
                    await dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> HandleSpotifyPlayerInitialization()
        {
            var isSpotifyInstalled = spotifyLoader.IsSpotifyInstalled();
            if (!isSpotifyInstalled)
            {
                if (currentPlatform == CurrentPlatform.iOS)
                {
                    if (!spotifyWebPlayerHasBeenLoaded)
                    {
                        await StartLoadingSpotifyWebPlayer();
                    }

                    if (spotifyWebPlayerHasBeenLoaded && !spotifyWebPlayerHasBeenActivated)
                    {
                        await StartSpotifyWebPlayerActivation();
                    }

                    if (spotifyWebPlayerHasBeenActivated && string.IsNullOrWhiteSpace(spotifyDeviceId))
                    {
                        spotifyDeviceId = await AwaitWebPlayerActivation();
                    }

                    if (string.IsNullOrWhiteSpace(spotifyDeviceId))
                    {
                        await dialogService.ShowAlertAsync(AppResources.Error_Content_NoActivePlayer, AppResources.Error_Caption);
                        IsBusy = false;
                        return false;
                    }
                }
                else
                {
                    await InitiateSpotifyInstallationAsync();
                    return false;
                }
            }

            await InitAudioPlayer(!isSpotifyInstalled, spotifyDeviceId);

            if (!spotifyApiService.UserPremiumCheckSuccessful)
            {
                var premiumCheck = await spotifyApiService.IsPremiumUser();
                IsPremiumUser = premiumCheck.Item2;

                if (!IsPremiumUser && premiumCheck.Item1.Equals(HttpStatusCode.OK))
                {
                    await dialogService.ShowAlertAsync(AppResources.Error_Content_NoPremiumUser, AppResources.Hint_Caption_General);
                    return false;
                }
                else if (!IsPremiumUser && !premiumCheck.Item1.Equals(HttpStatusCode.OK))
                {
                    await dialogService.ShowAlertAsync(AppResources.Error_Content_PremiumUserCheckFailed, AppResources.Hint_Caption_General);
                    return false;
                }
            }

            return true;
        }

        private void OnPlayingStatusChanged(object sender, bool e)
        {
            IsPlaying = e;
        }

        private void OnCurrentTimePositionChanged(object sender, EventArgs e)
        {
            try
            {
                UiContext.Send(x =>
                    {
                        CurrentAudioPlayer.GetCurrentPosition((o) =>
                        {
                            CurrentSongTime = FormatTime(o);
                        });
                    }, null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
            }
        }

        private string FormatTime(double time)
        {
            var result = TimeSpan.FromMilliseconds(0).ToString(@"mm\:ss");
            try
            {
                result = TimeSpan.FromMilliseconds(time).ToString(@"mm\:ss");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                dialogService.ShowAlertAsync(ex.Message);
            }

            return result;
        }

        private async void OnLoopChanged(SessionDetailsViewModel sender, LoopViewModel loop)
        {
            if (loop.Loop.Id != CurrentLoop.Id)
            {
                try
                {
                    if (CurrentAudioPlayer.IsPlaying)
                    {
                        CurrentAudioPlayer.Pause(false);
                    }

                    CurrentLoop = loop.Loop;
                    CurrentSession = Sessions.FirstOrDefault(s => s.Session.Id == loop.Loop.SessionId);

                    await ExecutePlayCommand(null);
                    await NavigationService?.GoBackAsync();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    await dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
                }
            }
        }

        private async void OnDeleteLoop(LoopViewModel sender, Loop loop)
        {
            try
            {
                if (loop.IsDefault)
                {
                    return; // don't delete the default loop!!
                }

                bool isCurrentlyPlaying = CurrentAudioPlayer != null && CurrentAudioPlayer.IsPlaying;
                if (isCurrentlyPlaying)
                {
                    CurrentAudioPlayer.Pause(false);
                }

                if (loop == CurrentLoop)
                {
                    CurrentLoop = CurrentSession.Session.Loops.First();
                }

                await loopsRepository.DeleteAsync(loop);
                CurrentSession.Session.Loops.Remove(loop);

                if (isCurrentlyPlaying)
                {
                    await CurrentAudioPlayer.InitAsync(CurrentLoop);
                    await CurrentAudioPlayer?.PlayAsync();
                }
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync(AppResources.Error_Content_CouldNotDeleteLoop, AppResources.Error_Caption);
            }
        }

        private async Task ExecuteNavigateToSettingsCommand()
        {
            if (CurrentAudioPlayer != null && CurrentAudioPlayer.IsPlaying)
            {
                CurrentAudioPlayer.Pause(true);
            }

            await NavigationService?.NavigateToAsync<SettingsViewModel>();
        }

        public void UpdateLoopStartPosition()
        {
            if (IsInitialized && CurrentLoop != null && CurrentLoop.StartPosition != MinimumValue)
            {
                CurrentLoop.StartPosition = MinimumValue;
                NotifyPropertyChanged(nameof(MinimumValue));
            }
        }

        public void UpdateLoopEndPosition()
        {
            if (IsInitialized && CurrentLoop != null && CurrentLoop.EndPosition != MaximumValue)
            {
                CurrentLoop.EndPosition = MaximumValue;
                NotifyPropertyChanged(nameof(MaximumValue));
            }
        }

        private async Task InitiateSpotifyInstallationAsync()
        {
            var installSpotify = await dialogService.ShowConfirmAsync(
                                    AppResources.Hint_Caption_SpotifyMissing,
                                    AppResources.Hint_Content_SpotifyMissing,
                                    AppResources.Cancel,
                                    AppResources.Ok);

            if (installSpotify)
            {
                spotifyLoader.InstallSpotify();
            }
        }

        private void OnSpotifyWebPlayerPlayerLoaded(object sender, bool success)
        {
            spotifyWebPlayerHasBeenLoaded = success;
            Task.Run(() => spotifyWebPlayerLoadedTokenSource?.TrySetResult(success));
        }

        private void OnSpotifyWebPlayerActivated(object sender, bool success)
        {
            spotifyWebPlayerHasBeenActivated = success;
            Task.Run(() => spotifyWebPlayerActivationTokenSource?.SetResult(success));
        }

        private async Task<bool> StartLoadingSpotifyWebPlayer()
        {
            if (spotifyWebPlayerHasBeenLoaded)
            {
                return true;
            }

            try
            {
                UiContext.Send(x => MessagingCenter.Send(this, MessengerKeys.WebViewInit), null);
                spotifyWebPlayerLoadedTokenSource = new TaskCompletionSource<bool>();
                UiContext.Send(x => MessagingCenter.Send(this, MessengerKeys.SpotifyLoadWebPlayer), null);
                spotifyWebPlayerHasBeenLoaded = await spotifyWebPlayerLoadedTokenSource.Task;
                IsSpotifyWebPlayerVisible = !spotifyWebPlayerHasBeenLoaded;
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
            }
            finally
            {
                IsSpotifyWebPlayerVisible = false;
            }

            return spotifyWebPlayerHasBeenLoaded;
        }

        private async Task<bool> StartSpotifyWebPlayerActivation()
        {
            try
            {
                if (!spotifyWebPlayerHasBeenLoaded)
                {
                    return false;
                }

                IsSpotifyWebPlayerVisible = true;
                spotifyWebPlayerActivationTokenSource = new TaskCompletionSource<bool>();
                UiContext.Send(x => MessagingCenter.Send(this, MessengerKeys.SpotifyActivatePlayer), null);
                spotifyWebPlayerHasBeenActivated = await spotifyWebPlayerActivationTokenSource.Task;
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
            }
            finally
            {
                IsSpotifyWebPlayerVisible = false;
            }

            return spotifyWebPlayerHasBeenLoaded && spotifyWebPlayerHasBeenActivated;
        }

        private async Task<string> AwaitWebPlayerActivation()
        {
            var retries = 4;
            string id = null;
            try
            {
                while (retries > 0)
                {
                    var activeDevices = await spotifyApiService.GetAvailableDevices();
                    id = activeDevices.FirstOrDefault(d => d.Name == "Mobile Web Player" && d.Type == "Smartphone" && d.IsActive)?.Id;

                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        break;
                    }

                    retries--;
                    await Task.Delay(1);
                }
            }
            catch (Exception)
            {
                retries--;
                await Task.Delay(1);
            }

            return id;
        }

        private void OnWebViewRefreshInitialized(object obj)
        {
            if (CurrentAudioPlayer != null && CurrentAudioPlayer.IsPlaying && CurrentAudioPlayer.UsesWebPlayer)
            {
                CurrentAudioPlayer.Pause(true);
            }
        }

        private void ExecuteToggleSpotifyWebPlayerCommand(object obj)
        {
            IsSpotifyWebPlayerVisible = !IsSpotifyWebPlayerVisible;
        }

        public bool IsSpotifyInstalled()
        {
            return spotifyLoader?.IsSpotifyInstalled() ?? false;
        }

        // todo: remove in next versions
        private async Task FetchSpotifyCovers()
        {
            try
            {
                var spotifySessions = Sessions?
                    .Where(s => s.Session.AudioSource.Type == AudioSourceType.Spotify && (string.IsNullOrWhiteSpace(s.Session.CoverSource) || string.IsNullOrWhiteSpace(s.Session.Artist)));

                if (spotifySessions != null && spotifySessions.Any())
                {
                    if (!spotifyLoader.Authorized)
                    {
                        await spotifyLoader.InitializeAsync();
                        await spotifyApiService.PauseCurrentPlayback();
                    }

                    if (!spotifyLoader.Authorized)
                    {
                        return;
                    }

                    foreach (var session in spotifySessions)
                        {
                            var trackId = session.Session.AudioSource.Source.Split(':')?.Last();
                            var trackDetails = await spotifyApiService.GetSpotifyTrackDetails(trackId);

                            if (string.IsNullOrWhiteSpace(session.Session.CoverSource))
                            {
                                UiContext.Send(x => session.CoverSource = trackDetails.ImageSmall, null);
                                session.Session.CoverSource = trackDetails.ImageSmall;
                            }

                            if (string.IsNullOrWhiteSpace(session.Session.Artist))
                            {
                                UiContext.Send(x => session.Artist = trackDetails.Artist, null);
                                session.Session.Artist = trackDetails.Artist;
                            }

                            await sessionsRepository.UpdateAsync(session.Session);
                        }
                }
            }
            catch (Exception)
            {

            }
        }

        private async Task OnUpdateSession(Session session)
        {
            try
            {
                await sessionsRepository.UpdateAsync(session);
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync(AppResources.Error_Content_UpdateSessionError, AppResources.Error_Caption);
            }
        }


        ~MainViewModel()
        {
            spotifyLoader.Disconnected -= OnSpotifyDisconnected;
        }
        #endregion
    }
}
