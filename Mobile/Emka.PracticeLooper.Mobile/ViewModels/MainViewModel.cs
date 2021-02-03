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
using Emka3.PracticeLooper.Config.Contracts.Features;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private IInterstitialAd interstitialAd;
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
        private readonly IFeatureRegistry featureRegistry;
        private Command playCommand;
        private Command createSessionCommand;
        private Command deleteSessionCommand;
        private Command pickSourceCommand;
        private Command navigateToSettingsCommand;
        private Command toggleSpotifyWebPlayerCommand;
        private Command refreshWebViewCommand;
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
        #endregion

        #region Ctor
        public MainViewModel(IInterstitialAd interstitialAd,
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
            IFeatureRegistry featureRegistry)
            : base(navigationService, logger, appTracker)
        {
            this.interstitialAd = interstitialAd ?? throw new ArgumentNullException(nameof(interstitialAd));
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
            this.featureRegistry = featureRegistry ?? throw new ArgumentNullException(nameof(featureRegistry));
            AudioPlayers = audioPlayers?.ToList() ?? throw new ArgumentNullException(nameof(audioPlayers));

            MessagingCenter.Subscribe<SpotifySearchViewModel, AudioSource>(this, MessengerKeys.NewTrackAdded, (sender, audioSorce) => CreateSessionCommand.Execute(audioSorce));
            MessagingCenter.Subscribe<SessionViewModel, SessionViewModel>(this, MessengerKeys.DeleteSession, (sender, arg) => DeleteSessionCommand.Execute(arg));
            MessagingCenter.Subscribe<SessionDetailsViewModel, LoopViewModel>(this, MessengerKeys.LoopChanged, OnLoopChanged);
            MessagingCenter.Subscribe<LoopViewModel, Loop>(this, MessengerKeys.DeleteLoop, OnDeleteLoop);
            MessagingCenter.Subscribe<object, bool>(this, MessengerKeys.SpotifyWebPlayerLoaded, OnSpotifyWebPlayerPlayerLoaded);
            MessagingCenter.Subscribe<object, bool>(this, MessengerKeys.SpotifyPlayerActivated, OnSpotifyWebPlayerActivated);

            Sessions = new ObservableCollection<SessionViewModel>();
            isPlaying = false;
            showCallToAction = VersionTracking.IsFirstLaunchEver ? true : false;
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
        public Command RefreshWebViewCommand => refreshWebViewCommand ??= new Command(ExecuteRefreshWebViewCommand);
        
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
            get => CurrentLoop != null ? FormatTime(minimumValue * CurrentLoop.Session.AudioSource.Duration * 1000) : string.Empty;
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

                    InitAudioPlayer();

                    if (configurationService.GetValue(PreferenceKeys.LastSession, default(int)) != currentSession.Session.Id)
                    {
                        configurationService.SetValue(PreferenceKeys.LastSession, currentSession.Session.Id, true);
                    }

                    if (isCurrentlyPlaying)
                    {
                        PlayCommand.Execute(null);
                    }
                }

                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsInitialized));
                NotifyPropertyChanged(nameof(IsPlaying));
                NotifyPropertyChanged(nameof(MinimumRange));
                NotifyPropertyChanged(nameof(MinimumValue));
                NotifyPropertyChanged(nameof(MaximumValue));
                PlayCommand.ChangeCanExecute();
                AddNewLoopCommand.ChangeCanExecute();
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
            if (UiContext == null)
            {
                throw new InvalidOperationException("UiContext is null!");
            }

            IsBusy = true;
            try
            {
                await sessionsRepository.InitAsync();
                await loopsRepository.InitAsync();
                var items = await sessionsRepository.GetAllItemsAsync().ConfigureAwait(false);
                spotifyLoader.Disconnected += OnSpotifyDisconnected;

                if (items != null && items.Any())
                {
                    UiContext?.Send(x =>
                            {
                                foreach (var item in items)
                                {
                                    Sessions.Add(new SessionViewModel(item, dialogService, Logger, NavigationService, Tracker));
                                }

                                CurrentSession = Sessions.FirstOrDefault(s => s.Session.Id == configurationService.GetValue(PreferenceKeys.LastSession, default(int))) ?? Sessions.FirstOrDefault();
                            }, null);
                }

                if (!connectivityService.HasFastConnection() && !configurationService.GetValue(PreferenceKeys.SlowConnectionWarning, default(bool)))
                {
                    await dialogService.ShowAlertAsync(AppResources.Hint_Content_SlowConnection, AppResources.Hint_Caption_SlowConnection);
                    await configurationService.SetValueAsync(PreferenceKeys.SlowConnectionWarning, true, true);
                }
            }
            catch (Exception ex)
            {
                await Logger?.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
            }
            finally
            {
                IsBusy = false;
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

        private void ExecuteRefreshWebViewCommand(object o)
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

                    if (!featureRegistry.IsEnabled<PremiumFeature>())
                    {
                        if (CurrentSession != null)
                        {
                            await sessionsRepository.DeleteAsync(CurrentSession.Session);
                            Sessions.Clear();
                        }

                        configurationService.SetValue(PreferenceKeys.LastSession, newSession.Id, true);
                        configurationService.SetValue(PreferenceKeys.LastLoop, newSession.Loops.First().Id, true);
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

                if (IsPlaying)
                {
                    UiContext.Send(x => IsPlaying = false, null);
                    UiContext.Send(x => CurrentAudioPlayer.Pause(true), null);
                    CurrentAudioPlayer.PlayStatusChanged -= OnPlayingStatusChanged;
                    CurrentAudioPlayer.CurrentTimePositionChanged -= OnCurrentTimePositionChanged;
                    await interstitialAd?.ShowAdAsync();
                }
                else
                {
                    CurrentAudioPlayer.PlayStatusChanged += OnPlayingStatusChanged;
                    CurrentAudioPlayer.CurrentTimePositionChanged += OnCurrentTimePositionChanged;
                    if (CurrentAudioPlayer.Types.HasFlag(AudioSourceType.Spotify))
                    {
                        if (!spotifyLoader.IsSpotifyInstalled())
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
                                var activeDevices = await spotifyApiService.GetAvailableDevices();
                                spotifyDeviceId = activeDevices.FirstOrDefault(d => d.Name == "Mobile Web Player" && d.Type == "Smartphone" && d.IsActive)?.Id;
                            }

                            if (string.IsNullOrWhiteSpace(spotifyDeviceId))
                            {
                                await dialogService.ShowAlertAsync(AppResources.Error_Content_NoActivePlayer, AppResources.Error_Caption);
                                IsBusy = false;
                                return;
                            }
                        }

                        if (!spotifyApiService.UserPremiumCheckSuccessful)
                        {
                            var premiumCheck = await spotifyApiService.IsPremiumUser();
                            IsPremiumUser = premiumCheck.Item2;

                            if (!IsPremiumUser && premiumCheck.Item1.Equals(HttpStatusCode.OK))
                            {
                                await dialogService.ShowAlertAsync(AppResources.Error_Content_NoPremiumUser, AppResources.Hint_Caption_General);
                                return;
                            }
                            else if (!IsPremiumUser && !premiumCheck.Item1.Equals(HttpStatusCode.OK))
                            {
                                await dialogService.ShowAlertAsync(AppResources.Error_Content_PremiumUserCheckFailed, AppResources.Hint_Caption_General);
                                return;
                            }
                        }
                    }

                    if (!CurrentAudioPlayer.Initialized)
                    {
                        try
                        {
                            var useWebPlayer = !spotifyLoader.IsSpotifyInstalled();
                            await CurrentAudioPlayer.InitAsync(CurrentLoop, useWebPlayer, spotifyDeviceId);
                        }
                        catch (FileNotFoundException ex)
                        {
                            await Logger.LogErrorAsync(ex);
                            var deleteFile = await dialogService.ShowConfirmAsync(AppResources.Error_Caption, AppResources.Error_Content_FileNotFound, AppResources.Cancel, AppResources.Ok);

                            if (deleteFile)
                            {
                                DeleteSessionCommand.Execute(CurrentSession);
                            }

                            return;
                        }
                        catch (Exception ex)
                        {
                            await Logger.LogErrorAsync(ex);
                            await dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
                            return;
                        }
                    }

                    await CurrentAudioPlayer.PlayAsync();
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
                await interstitialAd?.ShowAdAsync();
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

        public void InitAudioPlayer()
        {
            try
            {
                IsBusy = true;
                if (IsPlaying)
                {
                    CurrentAudioPlayer?.Pause(false);
                }

                if (CurrentSession != null)
                {
                    CurrentAudioPlayer = AudioPlayers.First(p => p.Types.HasFlag(CurrentSession.Session.AudioSource.Type));
                    CurrentLoop = CurrentSession.Session.Loops.FirstOrDefault(l => l.Id == configurationService.GetValue(PreferenceKeys.LastLoop, default(int))) ?? CurrentSession.Session.Loops.First(l => l.IsDefault);
                    MinimumValue = CurrentLoop.StartPosition;
                    MaximumValue = CurrentLoop.EndPosition;
                    SongDuration = FormatTime(CurrentSession.Session.AudioSource.Duration * 1000);
                    CurrentSongTime = FormatTime(CurrentSession.Session.AudioSource.Duration * 1000 * MinimumValue);
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
            Task.Run(() => spotifyWebPlayerLoadedTokenSource?.SetResult(success));
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
                IsSpotifyWebPlayerVisible = true;
                spotifyWebPlayerLoadedTokenSource = new TaskCompletionSource<bool>();
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

                spotifyWebPlayerActivationTokenSource = new TaskCompletionSource<bool>();
                MessagingCenter.Send(this, MessengerKeys.SpotifyActivatePlayer);
                spotifyWebPlayerHasBeenActivated = await spotifyWebPlayerActivationTokenSource.Task;
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
            }

            return spotifyWebPlayerHasBeenLoaded && spotifyWebPlayerHasBeenActivated;
        }

        private void ExecuteToggleSpotifyWebPlayerCommand(object obj)
        {
            IsSpotifyWebPlayerVisible = !IsSpotifyWebPlayerVisible;
        }

        ~MainViewModel()
        {
            spotifyLoader.Disconnected -= OnSpotifyDisconnected;
        }
        #endregion
    }
}
