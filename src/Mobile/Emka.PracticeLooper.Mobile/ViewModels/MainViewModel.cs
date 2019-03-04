// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Windows.Input;
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
        private readonly IFilePicker filePicker;
        private readonly IAudioPlayer audioPlayer;

        #region Ctor
        public MainViewModel(IFilePicker filePicker, IAudioPlayer audioPlayer)
        {
            this.filePicker = filePicker;
            this.audioPlayer = audioPlayer;
            filePicker.SourceSelected += OnAudioSourceSelected;
        }
        #endregion

        #region Properties
        public ICommand PlayCommand => new Command(ExecutePlayCommand, CanExecutePlayCommand);
        public ICommand SelectSourceCommand => new Command(ExecuteSelectSourceCommand, CanExecuteSelectSourceCommand);
        public string SelectedAudioSource { get; set; }
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

        private bool CanExecutePlayCommand(object arg)
        {
            return false;
        }

        private void ExecutePlayCommand(object obj)
        {
            throw new NotImplementedException();
        }

        private void OnAudioSourceSelected(object sender, AudioSource audioSource)
        {
            Console.WriteLine(audioSource.Source);
            this.audioPlayer.Play(audioSource);
        }
        #endregion
    }
}
