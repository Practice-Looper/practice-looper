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
using Emka3.PracticeLooper.Config.Feature;
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
        private IConfigurationService configurationService;
        private Mobile.Common.IFilePicker filePicker;
        private Command playCommand;
        private Command createSessionCommand;
        private Command deleteSessionCommand;
        private Command pickSourceCommand;
        private Command navigateToSettingsCommand;
        private SessionViewModel currentSession;
        private Loop currentLoop;
        private bool isPlaying;
        private double minimum;
        private double maximum;
        private double minimumValue;
        private double maximumValue;
        private string songDuration;
        private string currentSongTime;
        private string loopStartPosition;
        private string loopEndPosition;
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
            Mobile.Common.IFilePicker filePicker)
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
            Sessions = new ObservableCollection<SessionViewModel>();
            CurrentSession = null;
            isPlaying = false;
            Maximum = 1;
            MaximumValue = 1;
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
            get
            {
                return minimum;
            }

            set
            {
                minimum = value;
                NotifyPropertyChanged();
            }
        }

        public double MinimumValue
        {
            get => minimumValue;
            set
            {
                minimumValue = value;
                if (IsInitialized)
                {
                    LoopStartPosition = FormatTime(minimumValue * CurrentLoop.Session.AudioSource.Duration * 1000);
                }

                NotifyPropertyChanged();
            }
        }

        public double Maximum
        {
            get => maximum;
            set
            {
                maximum = value;
                NotifyPropertyChanged();
            }
        }

        public double MaximumValue
        {
            get => maximumValue;
            set
            {
                maximumValue = value;
                if (IsInitialized)
                {
                    LoopEndPosition = FormatTime(maximumValue * CurrentLoop.Session.AudioSource.Duration * 1000);
                }

                NotifyPropertyChanged();
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
            get => loopStartPosition;
            set
            {
                loopStartPosition = value;
                NotifyPropertyChanged();
            }
        }

        public string LoopEndPosition
        {
            get => loopEndPosition;
            set
            {
                loopEndPosition = value;
                NotifyPropertyChanged();
            }
        }

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

                if (currentSession != null)
                {
                    InitAudioPlayer();
                    if (Preferences.Get(PreferenceKeys.LastSession, default(int)) != currentSession.Session.Id)
                    {
                        Preferences.Set(PreferenceKeys.LastLoop, currentSession.Session.Id);
                    }
                }

                NotifyPropertyChanged();
                NotifyPropertyChanged("IsInitialized");
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
            }
            catch (Exception ex)
            {
                await Logger?.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnSpotifyDisconnected(object sender, EventArgs args)
        {
            if (CurrentAudioPlayer != null && CurrentAudioPlayer.IsPlaying)
            {
                Pause();
                await dialogService.ShowAlertAsync("Oops, lost connection to Spotify!");
            }
        }

        public void UpdateMinMaxValues()
        {
            if (CurrentSession != null)
            {
                try
                {
                    CurrentSession.Session.Loops[0].StartPosition = MinimumValue;
                    CurrentSession.Session.Loops[0].EndPosition = MaximumValue;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    dialogService.ShowAlertAsync(ex.Message);
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
                dialogService.ShowAlertAsync(ex.Message);
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
                    dialogService.ShowAlertAsync(ex.Message);
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
                                IsFavorite = true
                            }
                        }
                    };

                    newSession.Loops.First().Session = newSession;

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

                    await sessionsRepository.SaveAsync(newSession);
                    Sessions.Add(newSessionViewModel);
                    CurrentSession = newSessionViewModel;
                }
                catch (Exception ex)
                {
                    await Logger.LogErrorAsync(ex);
                    await dialogService.ShowAlertAsync(ex.Message);
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
                if (CurrentAudioPlayer.IsPlaying)
                {
                    Device.BeginInvokeOnMainThread(() => IsPlaying = false);
                    Device.BeginInvokeOnMainThread(CurrentAudioPlayer.Pause);
                    CurrentAudioPlayer.PlayStatusChanged -= OnPlayingStatusChanged;
                    CurrentAudioPlayer.CurrentTimePositionChanged -= OnCurrentTimePositionChanged;
                    CurrentAudioPlayer.TimerElapsed -= CurrentAudioPlayer_TimerElapsed;

                    await interstitialAd?.ShowAdAsync();
                }
                else
                {
                    CurrentAudioPlayer.PlayStatusChanged += OnPlayingStatusChanged;
                    CurrentAudioPlayer.CurrentTimePositionChanged += OnCurrentTimePositionChanged;
                    CurrentAudioPlayer.TimerElapsed += CurrentAudioPlayer_TimerElapsed;

                    if (!CurrentAudioPlayer.Initialized)
                    {
                        await CurrentAudioPlayer.InitAsync(CurrentLoop);
                    }

                    await CurrentAudioPlayer.PlayAsync();
                    Device.BeginInvokeOnMainThread(() => IsPlaying = true);
                }
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync(ex.Message);
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
                    await dialogService.ShowAlertAsync(ex.Message);
                }
            }
        }

        private async Task ExecutePickSourceCommandAsync(object o)
        {
            try
            {
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
                await dialogService.ShowAlertAsync("Oops, something went wrong!");
            }
        }

        private bool CanExecuteAddNewLoopCommand(object arg)
        {
            return CurrentSession != null;
        }

        private async Task ExecuteAddNewLoopCommand(object o)
        {
            try
            {
                var name = await dialogService.ShowPromptAsync("New loop", "Enter a name", "Save", "Cancel", $"Loop{CurrentSession.Session.Loops.Count + 1}");
                if (!string.IsNullOrEmpty(name))
                {
                    var loop = new Loop() { Name = name, StartPosition = MinimumValue, EndPosition = MaximumValue, Session = CurrentSession.Session, SessionId = CurrentSession.Session.Id };
                    loop.Id = await loopsRepository.SaveAsync(loop);
                    CurrentSession.Session.Loops.Add(loop);
                }
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync("Oops, failed to add loop. Please try again.");
            }
        }

        private void InitAudioPlayer()
        {
            try
            {
                if (IsPlaying)
                {
                    CurrentAudioPlayer.Pause();
                }

                if (CurrentSession != null)
                {
                    CurrentAudioPlayer = Factory.GetResolver().ResolveAll<IAudioPlayer>().First(p => p.Type == CurrentSession.Session.AudioSource.Type);
                    CurrentLoop = CurrentSession.Session.Loops.FirstOrDefault(l => l.Id == Preferences.Get(PreferenceKeys.LastLoop, default(int))) ?? CurrentSession.Session.Loops[0];
                    MinimumValue = CurrentLoop.StartPosition;
                    MaximumValue = CurrentLoop.EndPosition;
                    SongDuration = FormatTime(CurrentSession.Session.AudioSource.Duration * 1000);
                    CurrentSongTime = FormatTime(CurrentSession.Session.AudioSource.Duration * 1000 * MinimumValue);
                }
                else
                {
                    MinimumValue = 0.0;
                    MaximumValue = 1.0;
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
                dialogService.ShowAlertAsync(ex.Message);
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
                dialogService.ShowAlertAsync(ex.Message);
            }
        }

        private string FormatTime(double time)
        {
            try
            {
                return TimeSpan.FromMilliseconds(time).ToString(@"mm\:ss");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                dialogService.ShowAlertAsync(ex.Message);
            }

            return TimeSpan.FromMilliseconds(0).ToString(@"mm\:ss");
        }

        private void OnLoopChanged(LoopsDetailsViewModel sender, LoopViewModel loop)
        {
            try
            {
                var isCurrentlyPlaying = CurrentAudioPlayer?.IsPlaying;
                CurrentLoop = loop.Loop;
                InitAudioPlayer();

                if (isCurrentlyPlaying.HasValue && isCurrentlyPlaying.Value)
                {
                    CurrentAudioPlayer.Play();
                }
            }
            catch (Exception ex)
            {

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
                    CurrentLoop = CurrentSession.Session.Loops[0];
                    CurrentLoop.IsFavorite = true;
                    await loopsRepository.UpdateAsync(CurrentLoop);
                }
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync("Opps, failed to delete loop");
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
