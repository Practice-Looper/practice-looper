// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Mobile.Views;
using Emka.PracticeLooper.Model;
using Emka3.PracticeLooper.Model.Player;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Emka.PracticeLooper.Mobile.Common
{
    [Preserve(AllMembers = true)]
    public class SourcePicker : ISourcePicker
    {
        private readonly Page page;
        private AutoResetEvent autoResetEvent;
        private AudioSourceVieModel selectedSource;

        public SourcePicker()
        {
            page = Application.Current.MainPage;
            autoResetEvent = new AutoResetEvent(false);
            MessagingCenter.Subscribe<AudioSourceVieModel>(this, MessengerKeys.AudioSourceSelected, OnAudioSourceSelected);
            MessagingCenter.Subscribe<PickAudioSourceView>(this, MessengerKeys.AudioSourcePickerClosed, OnSourcePickerClosed);
        }

        public async Task<AudioSourceType> SelectFileSource()
        {
            var sources = new List<AudioSourceVieModel>
            {
                new AudioSourceVieModel(AppResources.Spotify, AudioSourceType.Spotify),
                new AudioSourceVieModel(AppResources.AudioFile, AudioSourceType.LocalInternal)
            };

            await PopupNavigation.Instance.PushAsync(new PickAudioSourceView(sources));
            await Task.Run(autoResetEvent.WaitOne);
            await PopupNavigation.Instance.PopAsync();
            return selectedSource.AudioType;
        }

        private void OnAudioSourceSelected(AudioSourceVieModel obj)
        {
            selectedSource = obj;
            autoResetEvent.Set();
        }

        private void OnSourcePickerClosed(PickAudioSourceView obj)
        {
            selectedSource = new AudioSourceVieModel(string.Empty, AudioSourceType.None);
            autoResetEvent.Set();
        }
    }
}
