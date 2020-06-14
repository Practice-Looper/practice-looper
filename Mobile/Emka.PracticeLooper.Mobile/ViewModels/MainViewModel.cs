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
using Factory = Emka3.PracticeLooper.Mappings.Factory;
using Emka3.PracticeLooper.Utils;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
using Xamarin.Essentials;
using Emka3.PracticeLooper.Config;

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
        private readonly IConfigurationService configurationService;
        private Mobile.Common.IFilePicker filePicker;
        private readonly IConnectivityService connectivityService;
        private Command playCommand;
        private Command createSessionCommand;
        private Command deleteSessionCommand;
        private Command pickSourceCommand;
        private Command navigateToSettingsCommand;
        private SessionViewModel currentSession;
        private Loop currentLoop;
        private bool isPlaying;
        private double minimumValue;
        private double maximumValue;
        private string songDuration;
        private string currentSongTime;
        private Command addNewLoopCommand;
        private bool isBusy;
        #endregion

        #region Ctor
        public MainViewModel(IInterstitialAd interstitialAd,
            IRepository<Session> sessionsRepository,
            IRepository<Loop> loopsRepository,
            IDialogService dialogService,
            IFileRepository fileRepository,
            ISourcePicker sourcePicker,
            ISpotifyLoader spotifyLoader,
            IConfigurationService configurationService,
            Mobile.Common.IFilePicker filePicker,
            IConnectivityService connectivityService)
        {
            this.interstitialAd = interstitialAd;
            this.sessionsRepository = sessionsRepository;
            this.loopsRepository = loopsRepository;
            this.dialogService = dialogService;
            this.fileRepository = fileRepository;
            this.sourcePicker = sourcePicker;
            this.spotifyLoader = spotifyLoader;
            this.configurationService = configurationService;
            this.filePicker = filePicker;
            this.connectivityService = connectivityService;
            Sessions = new ObservableCollection<SessionViewModel>();
            isPlaying = false;
            MessagingCenter.Subscribe<SpotifySearchViewModel, AudioSource>(this, MessengerKeys.NewTrackAdded, (sender, audioSorce) => CreateSessionCommand.Execute(audioSorce));
            MessagingCenter.Subscribe<SessionViewModel, SessionViewModel>(this, MessengerKeys.DeleteSession, (sender, arg) => DeleteSessionCommand.Execute(arg));
            MessagingCenter.Subscribe<LoopsDetailsViewModel, LoopViewModel>(this, MessengerKeys.LoopChanged, OnLoopChanged);
            MessagingCenter.Subscribe<LoopViewModel, Loop>(this, MessengerKeys.DeleteLoop, OnDeleteLoop);
        }

        public MainViewModel()
        {

        }
        #endregion

        #region Properties
        public Command DeleteSessionCommand => deleteSessionCommand ?? (deleteSessionCommand = new Command(async (o) => await ExecuteDeleteSessionCommandAsync(o)));

        public Command PlayCommand => playCommand ?? (playCommand = new Command(async (o) => await ExecutePlayCommand(o), CanExecutePlayCommand));

        public Command CreateSessionCommand => createSessionCommand ?? (createSessionCommand = new Command(async (o) => await ExecuteCreateSessionCommandAsync(o), CanExecuteCreateSessionCommand));

        public Command PickSourceCommand => pickSourceCommand ?? (pickSourceCommand = new Command(async (o) => await ExecutePickSourceCommandAsync(o)));

        public Command AddNewLoopCommand => addNewLoopCommand ?? (addNewLoopCommand = new Command(async (o) => await ExecuteAddNewLoopCommand(o), CanExecuteAddNewLoopCommand));

        public Command NavigateToSettingsCommand => navigateToSettingsCommand ?? (navigateToSettingsCommand = new Command(async () => await ExecuteNavigateToSettingsCommand()));

        public IAudioPlayer CurrentAudioPlayer { get; private set; }

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
            get => CurrentLoop != null ? CurrentLoop.StartPosition : default;
            set
            {
                minimumValue = Math.Round(value, 4);
                if (IsInitialized && CurrentLoop != null && CurrentLoop.StartPosition != value)
                {
                    CurrentLoop.StartPosition = minimumValue;
                }

                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(LoopStartPosition));
            }
        }

        public double Maximum
        {
            get => 1;
        }

        public double MaximumValue
        {
            get => CurrentLoop != null ? CurrentLoop.EndPosition : default;
            set
            {
                maximumValue = Math.Round(value, 4);
                if (IsInitialized && CurrentLoop != null && CurrentLoop.EndPosition != value)
                {
                    CurrentLoop.EndPosition = maximumValue;
                }

                NotifyPropertyChanged();
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
                    InitAudioPlayer();
                    if (Preferences.Get(PreferenceKeys.LastSession, default(int)) != currentSession.Session.Id)
                    {
                        Preferences.Set(PreferenceKeys.LastSession, currentSession.Session.Id);
                    }

                    if (isCurrentlyPlaying)
                    {
                        PlayCommand.Execute(null);
                    }
                }

                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsInitialized));
                NotifyPropertyChanged(nameof(IsPlaying));
                NotifyPropertyChanged(nameof(StepFrequency));
                NotifyPropertyChanged(nameof(TickFrequency));
                NotifyPropertyChanged(nameof(MinimumRange));
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
                    if (Preferences.Get(PreferenceKeys.LastLoop, default(int)) != currentLoop.Id)
                    {
                        Preferences.Set(PreferenceKeys.LastLoop, currentLoop.Id);
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

        public double StepFrequency => CurrentSession != null ? 1 / CurrentSession.Session.AudioSource.Duration : 0;
        public double TickFrequency => StepFrequency * 5;
        #endregion

        #region Metods
        public override async Task InitializeAsync(object parameter)
        {
            IsBusy = true;
            try
            {
                await sessionsRepository.InitAsync();
                await loopsRepository.InitAsync();
                var items = await sessionsRepository.GetAllItemsAsync().ConfigureAwait(false);
                spotifyLoader.Disconnected += OnSpotifyDisconnected;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var item in items)
                    {
                        Sessions.Add(new SessionViewModel(item));
                    }

                    CurrentSession = Sessions.FirstOrDefault(s => s.Session.Id == Preferences.Get(PreferenceKeys.LastSession, default(int))) ?? Sessions.FirstOrDefault();
                });

                if (!connectivityService.HasFastConnection())
                {
                    await dialogService.ShowAlertAsync(AppResources.Hint_Content_SlowConnection, AppResources.Hint_Caption_SlowConnection);
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
            if (CurrentAudioPlayer != null)
            {
                CurrentAudioPlayer?.Pause();
                CurrentAudioPlayer.PlayStatusChanged -= OnPlayingStatusChanged;
                CurrentAudioPlayer.CurrentTimePositionChanged -= OnCurrentTimePositionChanged;
                CurrentAudioPlayer.TimerElapsed -= CurrentAudioPlayer_TimerElapsed;
            }
        }

        public void UpdateMinMaxValues()
        {
            if (CurrentLoop != null)
            {
                try
                {
                    CurrentLoop.StartPosition = MinimumValue;
                    CurrentLoop.EndPosition = MaximumValue;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
                }
            }
        }

        public void Pause()
        {
            try
            {
                CurrentAudioPlayer?.Pause();
                spotifyLoader?.Disconnect();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
            }
        }

        private void CurrentAudioPlayer_TimerElapsed(object sender, EventArgs e)
        {
            if (CurrentLoop != null)
            {
                try
                {
                    Device.BeginInvokeOnMainThread(() =>
                            {
                                CurrentAudioPlayer?.Seek(CurrentLoop.StartPosition);
                            });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
                }
            }
        }

        private bool CanExecuteCreateSessionCommand(object arg)
        {
            return true;
        }

        private async Task ExecuteCreateSessionCommandAsync(object o)
        {
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

                    //newSession.Loops.First().Session = newSession;

                    var newSessionViewModel = new SessionViewModel(newSession);

                    if (!Emka3.PracticeLooper.Config.Factory.GetConfigService().GetValue<bool>(PreferenceKeys.PremiumGeneral))
                    {
                        if (CurrentAudioPlayer != null && CurrentAudioPlayer.IsPlaying)
                        {
                            await CurrentAudioPlayer.PauseAsync();
                        }

                        if (CurrentSession != null)
                        {
                            await sessionsRepository.DeleteAsync(CurrentSession.Session);
                            Sessions.Clear();
                        }

                        Preferences.Set(PreferenceKeys.LastSession, newSession.Id);
                        Preferences.Set(PreferenceKeys.LastLoop, newSession.Loops.First().Id);
                    }

                    newSession.Id = await sessionsRepository.SaveAsync(newSession);
                    Sessions.Add(newSessionViewModel);
                    CurrentSession = newSessionViewModel;
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

        private async Task ExecutePlayCommand(object o)
        {
            try
            {
                IsBusy = true;
                if (CurrentAudioPlayer.IsPlaying)
                {
                    Device.BeginInvokeOnMainThread(() => IsPlaying = false);
                    Device.BeginInvokeOnMainThread(() => CurrentAudioPlayer.Pause());
                    CurrentAudioPlayer.PlayStatusChanged -= OnPlayingStatusChanged;
                    CurrentAudioPlayer.CurrentTimePositionChanged -= OnCurrentTimePositionChanged;
                    CurrentAudioPlayer.TimerElapsed -= CurrentAudioPlayer_TimerElapsed;

                    await interstitialAd?.ShowAdAsync();
                }
                else
                {
                    if (!CurrentAudioPlayer.Initialized)
                    {
                        try
                        {
                            await CurrentAudioPlayer.InitAsync(CurrentLoop);
                        }
                        catch (Exception ex)
                        {
                            await dialogService.ShowAlertAsync(ex.Message, AppResources.Error_Caption);
                            return;
                        }
                    }

                    CurrentAudioPlayer.PlayStatusChanged += OnPlayingStatusChanged;
                    CurrentAudioPlayer.CurrentTimePositionChanged += OnCurrentTimePositionChanged;
                    CurrentAudioPlayer.TimerElapsed += CurrentAudioPlayer_TimerElapsed;

                    await CurrentAudioPlayer.PlayAsync();
                    Device.BeginInvokeOnMainThread(() => IsPlaying = true);
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

        private async Task ExecuteDeleteSessionCommandAsync(object session)
        {
            var tmpSession = session as SessionViewModel;
            if (session != null)
            {
                try
                {
                    if (IsPlaying && CurrentSession == tmpSession)
                    {
                        CurrentAudioPlayer.Pause();
                    }

                    await sessionsRepository.DeleteAsync(tmpSession.Session);

                    if (tmpSession.Session.AudioSource.Type == AudioSourceType.Local)
                    {
                        await fileRepository.DeleteFileAsync(tmpSession.Session.AudioSource.Source);
                    }

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Sessions.Remove(tmpSession);
                        if (Sessions.Any() && CurrentSession != null)
                        {
                            CurrentSession = Sessions.First();
                            CurrentLoop = currentSession.Session.Loops.First();
                        }
                        else
                        {
                            if (Preferences.Get(PreferenceKeys.LastLoop, -1) == CurrentLoop.Id)
                            {
                                Preferences.Clear(PreferenceKeys.LastLoop);
                            }

                            CurrentLoop = null;

                            if (Preferences.Get(PreferenceKeys.LastSession, -1) == CurrentSession.Session.Id)
                            {
                                Preferences.Clear(PreferenceKeys.LastSession);
                            }

                            CurrentSession = null;
                        }

                        NotifyPropertyChanged(nameof(IsInitialized));
                    });
                }
                catch (Exception ex)
                {
                    await Logger.LogErrorAsync(ex);
                    await dialogService.ShowAlertAsync(AppResources.Error_Content_CouldNotDeleteSong, AppResources.Error_Caption);
                }
            }
        }

        private async Task ExecutePickSourceCommandAsync(object o)
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() => IsBusy = true);
                var source = await sourcePicker?.SelectFileSource();
                await interstitialAd?.ShowAdAsync();

                switch (source)
                {
                    case AudioSourceType.Local:
                        var newFile = await filePicker.ShowPicker();
                        // file is null when user cancelled file picker!
                        if (newFile != null)
                        {
                            CreateSessionCommand.Execute(newFile);
                        }

                        break;
                    case AudioSourceType.Spotify:
                        if (spotifyLoader != null && !spotifyLoader.Authorized)
                        {
                            await spotifyLoader.InitializeAsync();
                        }

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
                MainThread.BeginInvokeOnMainThread(() => IsBusy = false);
            }
        }

        private bool CanExecuteAddNewLoopCommand(object arg)
        {
            return CurrentSession != null;
        }

        private async Task ExecuteAddNewLoopCommand(object o)
        {
            IsBusy = true;
            try
            {
                var name = await dialogService.ShowPromptAsync(AppResources.Prompt_Caption_NewLoop, AppResources.Prompt_Content_NewLoop, AppResources.Save, AppResources.Cancel, string.Format(AppResources.Prompt_Content_NewLoo_NamePlaceholder, CurrentSession.Session.Loops.Count + 1));
                if (!string.IsNullOrEmpty(name))
                {
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

        private void InitAudioPlayer()
        {
            try
            {
                IsBusy = true;
                if (IsPlaying)
                {
                    CurrentAudioPlayer.Pause();
                }

                if (CurrentSession != null)
                {
                    CurrentAudioPlayer = Factory.GetResolver().ResolveAll<IAudioPlayer>().First(p => p.Type == CurrentSession.Session.AudioSource.Type);
                    CurrentLoop = CurrentSession.Session.Loops.FirstOrDefault(l => l.Id == Preferences.Get(PreferenceKeys.LastLoop, default(int))) ?? CurrentSession.Session.Loops.First(l => l.IsDefault);
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
                Device.BeginInvokeOnMainThread(() =>
                    {
                        CurrentAudioPlayer.GetCurrentPosition((o) =>
                        {
                            CurrentSongTime = FormatTime(o);
                        });
                    });
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

        private async void OnLoopChanged(LoopsDetailsViewModel sender, LoopViewModel loop)
        {
            if (loop.Loop != CurrentLoop)
            {
                try
                {
                    if (CurrentAudioPlayer.IsPlaying)
                    {
                        CurrentAudioPlayer.Pause(false);
                    }

                    //MainThread.BeginInvokeOnMainThread(() =>
                    //{
                    //    MinimumValue = CurrentLoop.StartPosition;
                    //    MaximumValue = CurrentLoop.EndPosition;
                    //});

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
                await loopsRepository.DeleteAsync(loop);
                CurrentSession.Session.Loops.Remove(loop);

                if (loop == CurrentLoop)
                {
                    Pause();
                    CurrentLoop = CurrentSession.Session.Loops.First(l => l.IsDefault);
                    await loopsRepository.UpdateAsync(CurrentLoop);
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

        ~MainViewModel()
        {
            spotifyLoader.Disconnected -= OnSpotifyDisconnected;
        }
        #endregion
    }
}
