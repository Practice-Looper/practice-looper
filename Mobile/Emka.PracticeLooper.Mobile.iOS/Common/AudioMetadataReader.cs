// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using AVFoundation;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Foundation;
using Factory = Emka3.PracticeLooper.Mappings.Factory;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{

    [Preserve(AllMembers = true)]
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
                var dataSource = Factory.GetResolver().Resolve<IAudioFileLoader>()?.GetAbsoluteFilePath(audioSource);
                var asset = AVAsset.FromUrl(NSUrl.FromFilename(dataSource));
                var metaData = new AudioMetadata((int)asset.Duration.Seconds);
                return metaData;
            });
        }
        #endregion
    }
}
