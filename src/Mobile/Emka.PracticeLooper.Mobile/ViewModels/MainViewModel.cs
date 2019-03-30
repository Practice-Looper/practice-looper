// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
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
        private readonly IList<Session> sessions;
        private Command playCommand;
        private Command selectSourceCommand;
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
        private bool isInitialized;
        #endregion

        #region Ctor
        public MainViewModel(IFilePicker filePicker, IAudioPlayer audioPlayer, ISourcePicker sourcePicker)
        {
            this.filePicker = filePicker;
            this.audioPlayer = audioPlayer;
            this.sourcePicker = sourcePicker;
            sessions = new List<Session>();
            isPlaying = false;
            audioPlayer.PlayStatusChanged += OnPlayingStatusChanged;
            audioPlayer.CurrentTimePositionChanged += OnCurrentTimePositionChanged;
            Maximum = 1;
            MaximumValue = 1;
        }
        #endregion

        #region Properties
        public Command PlayCommand => playCommand ?? (playCommand = new Command(ExecutePlayCommand, CanExecutePlayCommand));
        public Command SelectSourceCommand => selectSourceCommand ?? (selectSourceCommand = new Command(async (o) => await ExecuteSelectSourceCommandAsync(), CanExecuteSelectSourceCommand));
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
                if (sessions.Any())
                {
                    sessions[0].Loops[0].StartPosition = value;
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
                if (sessions.Any())
                {
                    sessions[0].Loops[0].EndPosition = value;
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

        public bool IsInitialized => SelectedAudioSource != null;
        #endregion

        #region Metods
        private bool CanExecuteSelectSourceCommand(object arg)
        {
            return true;
        }

        private async Task ExecuteSelectSourceCommandAsync()
        {
            var sorceType = await sourcePicker.SelectFileSource();
            switch (sorceType)
            {
                case "File":
                    SelectedAudioSource = await filePicker.ShowPicker();
                    break;
                default:
                    break;
            }

            if (SelectedAudioSource != null)
            {
                InitAudioSourceSelected();
            }
        }

        private bool CanExecutePlayCommand(object o)
        {
            return SelectedAudioSource != null;
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

            sessions.Add(new Session("Coumarin", SelectedAudioSource, new List<Loop>
            {
                new Loop("My Loop", 0.5, 0.8, 0)
            }));

            audioPlayer.Init(sessions[0]);
            Minimum = 0;
            MinimumValue = sessions[0].Loops[0].StartPosition;
            Maximum = audioPlayer.SongDuration;
            MaximumValue = sessions[0].Loops[0].EndPosition;
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
        #endregion
    }
}
