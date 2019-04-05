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
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private readonly IFilePicker filePicker;
        private readonly IAudioPlayer audioPlayer;
        private readonly ISourcePicker sourcePicker;
        private Command playCommand;
        private Command createSessionCommand;
        private bool isPlaying;
        private double minimum;
        private double maximum;
        private double minimumValue;
        private double maximumValue;
        private IAudioSource selectedAudioSource;
        private string songDuration;
        private string currentSongTime;
        private string loopStartPosition;
        private string loopEndPosition;
        private Session currentSession;
        #endregion

        #region Ctor
        public MainViewModel(IFilePicker filePicker, IAudioPlayer audioPlayer, ISourcePicker sourcePicker)
        {
            this.filePicker = filePicker;
            this.audioPlayer = audioPlayer;
            this.sourcePicker = sourcePicker;
            Sessions = new ObservableCollection<Session>();
            CurrentSession = null;
            isPlaying = false;
            audioPlayer.PlayStatusChanged += OnPlayingStatusChanged;
            audioPlayer.CurrentTimePositionChanged += OnCurrentTimePositionChanged;
            Maximum = 1;
            MaximumValue = 1;
        }
        #endregion

        #region Properties
        public Command PlayCommand => playCommand ?? (playCommand = new Command(ExecutePlayCommand, CanExecutePlayCommand));
        public Command CreateSessionCommand => createSessionCommand ?? (createSessionCommand = new Command(async (o) => await ExecuteCreateSessionCommandAsync(), CanExecuteCreateSessionCommand));
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
        private bool CanExecuteCreateSessionCommand(object arg)
        {
            return true;
        }

        private async Task ExecuteCreateSessionCommandAsync()
        {
            var sorceType = await sourcePicker.SelectFileSource();
            switch (sorceType)
            {
                case "File":
                    var newFile = await filePicker.ShowPicker();
                    Sessions.Add(new Session(newFile.FileName, newFile, new List<Loop> { new Loop(newFile.FileName, 0.0, 1.0, 0) }));
                    break;
                default:
                    break;
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

        private void InitAudioSourceSelected()
        {
            if (IsPlaying)
            {
                audioPlayer.Pause();
            }

            audioPlayer.Init(CurrentSession);
            Minimum = 0;
            MinimumValue = CurrentSession.Loops[0].StartPosition;
            Maximum = audioPlayer.SongDuration;
            MaximumValue = CurrentSession.Loops[0].EndPosition;
            SongDuration = FormatTime(audioPlayer.SongDuration);
            CurrentSongTime = FormatTime(audioPlayer.SongDuration * MinimumValue);
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
