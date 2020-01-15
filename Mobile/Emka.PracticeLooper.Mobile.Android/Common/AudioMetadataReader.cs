// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Android.Media;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class AudioMetadataReader : IAudioMetadataReader
    {
        #region Ctor
        public AudioMetadataReader()
        {
        }
        #endregion

        #region Methods

        public async Task<IAudioMetaData> GetMetaDataAsync(Emka3.PracticeLooper.Model.Player.AudioSource audioSource)
        {
            var player = new MediaPlayer();
            player.Reset();
            player.SetDataSource(audioSource.Source);
            player.Prepare();
            return await Task.FromResult(new AudioMetadata(TimeSpan.FromMilliseconds(player.Duration).TotalSeconds));
        }
        #endregion
    }
}
