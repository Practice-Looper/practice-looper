// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Messenger;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private readonly IFilePicker filePicker;
        private readonly IAudioPlayer audioPlayer;
        private readonly IList<Session> sessions;
        private Command playCommand;
        private Command selectSourceCommand;
        private bool isPlaying;
        private double minimum;
        private double maximum;
        private double minumumValue;
        private double maximumValue;
        #endregion

        #region Ctor
        public MainViewModel(IFilePicker filePicker, IAudioPlayer audioPlayer)
        {
            this.filePicker = filePicker;
            this.audioPlayer = audioPlayer;
            sessions = new List<Session>();
            isPlaying = false;
            filePicker.SourceSelected += OnAudioSourceSelected;
            audioPlayer.PlayStatusChanged += OnPlayingStatusChanged;
            Maximum = 1;
            MaximumValue = 1;
        }
        #endregion

        #region Properties
        public Command PlayCommand => playCommand ?? (playCommand = new Command(ExecutePlayCommand, CanExecutePlayCommand));
        public Command SelectSourceCommand => selectSourceCommand ?? (selectSourceCommand = new Command(ExecuteSelectSourceCommand, CanExecuteSelectSourceCommand));
        public FileAudioSource SelectedAudioSource { get; set; }

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
            get => minumumValue;
            set
            {
                minumumValue = value;
                if (sessions.Any())
                {
                    sessions[0].Loops[0].StartPosition = value;
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
                }
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Metods
        private bool CanExecuteSelectSourceCommand(object arg)
        {
            return true;
        }

        private void ExecuteSelectSourceCommand(object obj)
        {
            MessagingCenter.Send(this, MessengerKeys.GetAudioSource);
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

        private void OnAudioSourceSelected(object sender, FileAudioSource audioSource)
        {
            if (IsPlaying)
            {
                audioPlayer.Pause();
            }

            sessions.Add(new Session("Coumarin", audioSource, new List<Loop>
            {
                new Loop("My Loop", 0.0, 1.0, 0)
            }));

            SelectedAudioSource = audioSource;
            audioPlayer.Init(sessions[0]);
            Minimum = 0;
            MinimumValue = sessions[0].Loops[0].StartPosition;
            Maximum = audioPlayer.SongDuration;
            MaximumValue = sessions[0].Loops[0].EndPosition;
            PlayCommand.ChangeCanExecute();
        }

        private void OnPlayingStatusChanged(object sender, bool e)
        {
            IsPlaying = e;
        }
        #endregion
    }
}
