// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.IO;
using System.Threading.Tasks;
using AVFoundation;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{

    [Preserve(AllMembers = true)]
    public class AudioMetadataReader : IAudioMetadataReader
    {
        #region Fields
        private readonly IConfigurationService configurationService;
        #endregion

        #region Ctor
        public AudioMetadataReader(IConfigurationService configurationService)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }
        #endregion

        #region Methods

        public async Task<IAudioMetaData> GetMetaDataAsync(AudioSource audioSource)
        {
            return await Task.Run(() =>
            {
                var asset = AVAsset.FromUrl(NSUrl.FromFilename(Path.Combine(configurationService.LocalPath, audioSource.Source)));
                var metaData = new AudioMetadata((int)asset.Duration.Seconds);
                return metaData;
            });
        }
        #endregion
    }
}
