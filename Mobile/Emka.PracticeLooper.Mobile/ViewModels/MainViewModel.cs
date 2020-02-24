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
using Emka3.PracticeLooper.Config.Feature;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using MediaManager;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields
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
                    LoopStartPosition = FormatTime(minimumValue * CurrentAudioPlayer.SongDuration);
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
                    LoopEndPosition = FormatTime(maximumValue * CurrentAudioPlayer.SongDuration);
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
                audioPlayers = Factory.GetResolver().ResolveAll<IAudioPlayer>().ToDictionary(player => player.Type);
                interstitialAd = Factory.GetResolver().Resolve<IInterstitialAd>();
                sessionsRepository = Factory.GetResolver().Resolve<IRepository<Session>>();
                sessionsRepository.Init();
                fileRepository = Factory.GetResolver().Resolve<IFileRepository>();
                filePicker = Factory.GetResolver().Resolve<Mobile.Common.IFilePicker>();
                spotifyLoader = Factory.GetResolver().Resolve<ISpotifyLoader>();
                var items = await sessionsRepository.GetAllItemsAsync().ConfigureAwait(false);
                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var item in items)
                    {
                        Sessions.Add(item);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void UpdateMinMaxValues()
        {
            if (CurrentSession != null)
            {
                CurrentSession.Loops[0].StartPosition = MinimumValue;
                CurrentSession.Loops[0].EndPosition = MaximumValue;
            }
        }

        private void CurrentAudioPlayer_TimerElapsed(object sender, EventArgs e)
        {
            if (CurrentLoop != null)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    CurrentAudioPlayer.Seek(CurrentLoop.StartPosition);
                });
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
                await sessionsRepository.SafeAsync(newSession);
            }
        }

        private bool CanExecutePlayCommand(object o)
        {
            return CurrentSession != null && CurrentAudioPlayer != null;
        }

        private async Task ExecutePlayCommand(object o)
        {
            if (IsPlaying)
            {
                Device.BeginInvokeOnMainThread(CurrentAudioPlayer.Pause);
            }
            else
            {
                if (!spotifyLoader.Authorized)
                {
                    await spotifyLoader.InitializeAsync(CurrentSession.AudioSource.Source);
                }

                await CurrentAudioPlayer.PlayAsync();
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

                }
            }
        }

        private async Task ExecutePickSourceCommandAsync(object o)
        {
            sourcePicker = Factory.GetResolver().Resolve<ISourcePicker>();
            var source = await sourcePicker.SelectFileSource();
            await interstitialAd.ShowAdAsync();

            switch (source)
            {
                case "File":
                    var newFile = await filePicker.ShowPicker();
                    // file is null when user cancelled file picker!
                    if (newFile != null)
                    {
                        CreateSessionCommand.Execute(newFile);
                    }

                    break;
                case "Spotify":
                    var canNavigate = true;
                    if (canNavigate)
                    {
                        await Task.Run(() => spotifyLoader.Initialize());
                        await NavigationService.NavigateToAsync<SpotifySearchViewModel>();
                    }

                    break;
                default:
                    break;
            }
        }

        private void InitAudioSourceSelected()
        {
            if (IsPlaying)
            {
                CurrentAudioPlayer.Pause();
            }

            if (CurrentSession != null)
            {
                CurrentLoop = CurrentSession.Loops[0];
                CurrentAudioPlayer = audioPlayers[CurrentSession.AudioSource.Type];
                CurrentAudioPlayer.PlayStatusChanged += OnPlayingStatusChanged;
                CurrentAudioPlayer.CurrentTimePositionChanged += OnCurrentTimePositionChanged;
                CurrentAudioPlayer.TimerElapsed += CurrentAudioPlayer_TimerElapsed;
                CurrentAudioPlayer.Init(CurrentLoop);
                MinimumValue = CurrentLoop.StartPosition;
                Maximum = CurrentAudioPlayer.SongDuration;
                MaximumValue = CurrentLoop.EndPosition;
                SongDuration = FormatTime(CurrentAudioPlayer.SongDuration);
                CurrentSongTime = FormatTime(CurrentAudioPlayer.SongDuration * MinimumValue);
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

        private void OnPlayingStatusChanged(object sender, bool e)
        {
            IsPlaying = e;
        }

        private void OnCurrentTimePositionChanged(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                CurrentAudioPlayer.GetCurrentPosition((o) =>
                {
                    CurrentSongTime = FormatTime(o);
                });
            });
        }

        private string FormatTime(double time)
        {
            return TimeSpan.FromMilliseconds(time).ToString(@"mm\:ss");
        }
        #endregion
    }
}
