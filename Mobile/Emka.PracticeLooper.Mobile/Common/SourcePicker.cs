// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Messenger;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Mobile.Views;
using Emka3.PracticeLooper.Config.Feature;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;
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
            var sources = new List<AudioSourceVieModel>();
            if (FeatureRegistry.IsEnabled<IPremiumAudioPlayer>("Spotify"))
            {
                sources.Add(new AudioSourceVieModel("Spotify", AudioSourceType.Spotify));
            }

            if (sources.Any())
            {
                sources.Add(new AudioSourceVieModel("Audio File", AudioSourceType.Local));
            }
            else
            {
                return AudioSourceType.Local;
            }
            
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

        private async void OnSourcePickerClosed(PickAudioSourceView obj)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}
