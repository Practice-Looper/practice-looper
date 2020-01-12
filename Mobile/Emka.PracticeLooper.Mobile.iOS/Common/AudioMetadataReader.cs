// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using AVFoundation;
using CoreMedia;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class AudioMetadataReader : IAudioMetadataReader
    {
        #region Ctor
        public AudioMetadataReader()
        {
        }
        #endregion

        #region Methods

        public async Task<IAudioMetaData> GetMetaDataAsync(AudioSource audioSource)
        {
            return await Task.Run(() =>
            {
                var asset = AVAsset.FromUrl(NSUrl.FromFilename(audioSource.Source));
                var metaData = new AudioMetadata((int)asset.Duration.Seconds);
                return metaData;
            });
        }
        #endregion
    }
}
