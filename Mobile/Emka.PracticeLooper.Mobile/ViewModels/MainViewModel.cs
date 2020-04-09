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
using Emka.PracticeLooper.Mobile.Messenger;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Forms;
using Factory = Emka3.PracticeLooper.Mappings.Factory;
using Microsoft.AppCenter.Crashes;
using Emka3.PracticeLooper.Utils;
using Emka3.PracticeLooper.Config.Feature;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private IConfigurationService configService;
        private IDictionary<AudioSourceType, IAudioPlayer> audioPlayers;
        private IInterstitialAd interstitialAd;
        private IRepository<Session> sessionsRepository;
        private IFileRepository fileRepository;
        private ISourcePicker sourcePicker;
        private ISpotifyLoader spotifyLoader;
        private Mobile.Common.IFilePicker filePicker;
        private Command playCommand;
        private Command createSessionCommand;
        private Command deleteSessionCommand;
        private Command pickSourceCommand;
        private Session currentSession;
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
        #endregion

        #region Ctor
        public MainViewModel()
        {
            Sessions = new ObservableCollection<Session>();
            CurrentSession = null;
            isPlaying = false;
            Maximum = 1;
            MaximumValue = 1;

            MessagingCenter.Subscribe<Session>(this, MessengerKeys.NewTrackAdded, (session) =>
            {
                Sessions.Add(session);
            });
        }
        #endregion

        #region Properties
        public Command DeleteSessionCommand => deleteSessionCommand ?? (deleteSessionCommand = new Command(async (o) => await ExecuteDeleteSessionCommandAsync(o)));

        public Command PlayCommand => playCommand ?? (playCommand = new Command(async (o) => await ExecutePlayCommand(o), CanExecutePlayCommand));

        public Command CreateSessionCommand => createSessionCommand ?? (createSessionCommand = new Command(async (o) => await ExecuteCreateSessionCommandAsync(o), CanExecuteCreateSessionCommand));

        public Command PickSourceCommand => pickSourceCommand ?? (pickSourceCommand = new Command(async (o) => await ExecutePickSourceCommandAsync(o)));

        private IAudioPlayer CurrentAudioPlayer { get; set; }

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
                if (Sessions.Any())
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
                if (Sessions.Any())
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

        public ObservableCollection<Session> Sessions { get; set; }

        public Session CurrentSession
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
                    InitAudioSourceSelected();
                    NotifyPropertyChanged("IsInitialized");
                }

                PlayCommand.ChangeCanExecute();
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
            }
        }
        #endregion

        #region Metods
        public override async Task InitializeAsync(object parameter)
        {
            try
            {
                if (FeatureRegistry.IsEnabled<IPremiumAudioPlayer>("Spotify"))
                {
                    spotifyLoader = Factory.GetResolver().Resolve<ISpotifyLoader>();
                }

                audioPlayers = Factory.GetResolver().ResolveAll<IAudioPlayer>().ToDictionary(player => player.Type);
                sourcePicker = Factory.GetResolver().Resolve<ISourcePicker>();
                interstitialAd = Factory.GetResolver().Resolve<IInterstitialAd>();
                sessionsRepository = Factory.GetResolver().Resolve<IRepository<Session>>();
                await sessionsRepository.InitAsync();
                fileRepository = Factory.GetResolver().Resolve<IFileRepository>();
                filePicker = Factory.GetResolver().Resolve<Mobile.Common.IFilePicker>();
                var items = await sessionsRepository.GetAllItemsAsync().ConfigureAwait(false);
                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var item in items)
                    {
                        Sessions.Add(item);
                    }
                });

                // check app and purchase status
            }
            catch (Exception ex)
            {
                await Logger?.LogErrorAsync(ex);
                await ShowErrorDialogAsync(ex);
            }
        }

        public void UpdateMinMaxValues()
        {
            if (CurrentSession != null)
            {
                try
                {
                    CurrentSession.Loops[0].StartPosition = MinimumValue;
                    CurrentSession.Loops[0].EndPosition = MaximumValue;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    ShowErrorDialog(ex);
                }
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
                    ShowErrorDialog(ex);
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
                                    Repititions = 0
                                }
                            }
                    };

                    Sessions.Add(newSession);
                    await sessionsRepository.SaveAsync(newSession);
                }
                catch (Exception ex)
                {
                    await Logger.LogErrorAsync(ex);
                    await ShowErrorDialogAsync();
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
                }
                else
                {
                    // todo: detach handler
                    try
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
                    catch (Exception ex)
                    {
                        Crashes.TrackError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(ex);
                await ShowErrorDialogAsync();
            }
        }

        private async Task ExecuteDeleteSessionCommandAsync(object session)
        {
            var tmpSession = session as Session;
            if (session != null)
            {
                try
                {
                    if (IsPlaying && CurrentSession == tmpSession)
                    {
                        CurrentAudioPlayer.Pause();
                    }

                    await fileRepository.DeleteFileAsync(tmpSession.AudioSource.Source);
                    await sessionsRepository.DeleteAsync(tmpSession);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Sessions.Remove(tmpSession);
                        if (Sessions.Any() && CurrentSession != null)
                        {
                            CurrentSession = Sessions.First();
                        }
                        else
                        {
                            CurrentSession = null;
                        }
                    });
                }
                catch (Exception ex)
                {
                    await Logger.LogErrorAsync(ex);
                    await ShowErrorDialogAsync();
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
                await ShowErrorDialogAsync();
            }
        }

        private void InitAudioSourceSelected()
        {
            try
            {
                if (IsPlaying)
                {
                    CurrentAudioPlayer.Pause();
                }

                if (CurrentSession != null)
                {
                    CurrentAudioPlayer = audioPlayers[CurrentSession.AudioSource.Type];
                    CurrentLoop = CurrentSession.Loops[0];
                    MinimumValue = CurrentLoop.StartPosition;
                    MaximumValue = CurrentLoop.EndPosition;
                    SongDuration = FormatTime(CurrentLoop.Session.AudioSource.Duration * 1000);
                    CurrentSongTime = FormatTime(CurrentLoop.Session.AudioSource.Duration * 1000 * MinimumValue);
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
                ShowErrorDialog();
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
                ShowErrorDialog();
            }

            return TimeSpan.FromMilliseconds(0).ToString(@"mm\:ss");
        }
        #endregion
    }
}
