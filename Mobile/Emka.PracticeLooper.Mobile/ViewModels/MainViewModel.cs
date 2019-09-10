// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Messenger;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private readonly IDictionary<AudioSourceType, IAudioPlayer> audioPlayers;
        private readonly IRepository<Session> sessionsRepository;
        private readonly IFileRepository fileRepository;
        private readonly ISpotifyLoader spotifyLoader;
        private Command playCommand;
        private Command createSessionCommand;
        private Command deleteSessionCommand;
        private Session currentSession;
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
        public MainViewModel(
            IDictionary<AudioSourceType, IAudioPlayer> audioPlayers,
            IRepository<Session> sessionsRepository,
            IFileRepository fileRepository,
            ISpotifyLoader spotifyLoader)
        {
            this.audioPlayers = audioPlayers;
            this.sessionsRepository = sessionsRepository;
            this.fileRepository = fileRepository;
            this.spotifyLoader = spotifyLoader;
            Sessions = new ObservableCollection<Session>();
            CurrentSession = null;
            isPlaying = false;
            Maximum = 1;
            MaximumValue = 1;

            MessagingCenter.Subscribe<Session>(this, MessengerKeys.NewTrackAdded, (session) =>
            {
                Sessions.Add(session);
            });

            Task.Run(async () => await Init());
        }
        #endregion

        #region Properties
        public Command DeleteSessionCommand => deleteSessionCommand ?? (deleteSessionCommand = new Command(async (o) => await ExecuteDeleteSessionCommandAsync(o)));

        public Command PlayCommand => playCommand ?? (playCommand = new Command(ExecutePlayCommand, CanExecutePlayCommand));

        public Command CreateSessionCommand => createSessionCommand ?? (createSessionCommand = new Command(async (o) => await ExecuteCreateSessionCommandAsync(o), CanExecuteCreateSessionCommand));

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
        #endregion

        #region Metods
        public async Task Init()
        {
            try
            {
                var items = await sessionsRepository.GetAllItemsAsync().ConfigureAwait(false);
                //await spotifyLoader.Initialize();
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

        private void CurrentAudioPlayer_TimerElapsed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                CurrentAudioPlayer.Seek(0);
            });
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
            return CurrentSession != null;
        }

        private void ExecutePlayCommand(object o)
        {
            if (IsPlaying)
            {
                CurrentAudioPlayer.Pause();
            }
            else
            {
                CurrentAudioPlayer.Play();
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

        private void InitAudioSourceSelected()
        {
            if (IsPlaying)
            {
                CurrentAudioPlayer.Pause();
            }

            if (CurrentSession != null)
            {
                CurrentAudioPlayer = audioPlayers[CurrentSession.AudioSource.Type];
                CurrentAudioPlayer.PlayStatusChanged += OnPlayingStatusChanged;
                CurrentAudioPlayer.CurrentTimePositionChanged += OnCurrentTimePositionChanged;
                CurrentAudioPlayer.TimerElapsed += CurrentAudioPlayer_TimerElapsed;
                CurrentAudioPlayer.Init(CurrentSession);
                MinimumValue = CurrentSession.Loops[0].StartPosition;
                Maximum = CurrentAudioPlayer.SongDuration;
                MaximumValue = CurrentSession.Loops[0].EndPosition;
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

        void OnCurrentTimePositionChanged(object sender, int e)
        {
            CurrentSongTime = FormatTime(e);
        }

        string FormatTime(double time)
        {
            return TimeSpan.FromMilliseconds(time).ToString(@"mm\:ss");
        }

        public void UpdateMinMaxValues()
        {
            if (CurrentSession != null)
            {
                CurrentSession.Loops[0].StartPosition = MinimumValue;
                CurrentSession.Loops[0].EndPosition = MaximumValue;
            }
        }
        #endregion
    }
}
