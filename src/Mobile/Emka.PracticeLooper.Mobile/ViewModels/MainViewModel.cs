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
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Xamarin.Forms;
using IFilePicker = Emka.PracticeLooper.Mobile.Common.IFilePicker;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private readonly IAudioPlayer audioPlayer;
        private readonly IRepository<Session> sessionsRepository;
        private readonly IFileRepository fileRepository;
        private readonly ISpotifyApiService spotifyApiService;
        private IAudioSource selectedAudioSource;
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
        public MainViewModel(IAudioPlayer audioPlayer,   
            IRepository<Session> sessionsRepository,
            IFileRepository fileRepository,
            ISpotifyApiService spotifyApiService)
        {
            this.audioPlayer = audioPlayer;
            this.sessionsRepository = sessionsRepository;
            this.fileRepository = fileRepository;
            this.spotifyApiService = spotifyApiService;
            Sessions = new ObservableCollection<Session>();
            CurrentSession = null;
            isPlaying = false;
            audioPlayer.PlayStatusChanged += OnPlayingStatusChanged;
            audioPlayer.CurrentTimePositionChanged += OnCurrentTimePositionChanged;
            Maximum = 1;
            MaximumValue = 1;

            Task.Run(async () => await Init());
        }
        #endregion

        #region Properties
        public Command DeleteSessionCommand => deleteSessionCommand ?? (deleteSessionCommand = new Command(async (o) => await ExecuteDeleteSessionCommandAsync(o)));
        public Command PlayCommand => playCommand ?? (playCommand = new Command(ExecutePlayCommand, CanExecutePlayCommand));
        public Command CreateSessionCommand => createSessionCommand ?? (createSessionCommand = new Command(async (o) => await ExecuteCreateSessionCommandAsync(o), CanExecuteCreateSessionCommand));
        public IAudioSource SelectedAudioSource
        {
            get => selectedAudioSource;
            set
            {
                selectedAudioSource = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsInitialized");
                PlayCommand.ChangeCanExecute();
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
                    LoopStartPosition = FormatTime(minimumValue * audioPlayer.SongDuration);
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
                    LoopEndPosition = FormatTime(maximumValue * audioPlayer.SongDuration);
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
                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var item in items)
                    {
                        Sessions.Add(item);
                    }

                    InitAudioSourceSelected();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
            return CurrentSession != null;
        }

        private void ExecutePlayCommand(object o)
        {
            if (IsPlaying)
            {
                audioPlayer.Pause();
            }
            else
            {
                audioPlayer.Play();
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
                        audioPlayer.Pause();
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
                audioPlayer.Pause();
            }

            if (CurrentSession != null)
            {
                audioPlayer.Init(CurrentSession);
                MinimumValue = CurrentSession.Loops[0].StartPosition;
                Maximum = audioPlayer.SongDuration;
                MaximumValue = CurrentSession.Loops[0].EndPosition;
                SongDuration = FormatTime(audioPlayer.SongDuration);
                CurrentSongTime = FormatTime(audioPlayer.SongDuration * MinimumValue);
            }
            else
            {
                MinimumValue = 0.0;
                MaximumValue = 1.0;
                SongDuration = FormatTime(0.0);
                CurrentSongTime = FormatTime(0.0);
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
