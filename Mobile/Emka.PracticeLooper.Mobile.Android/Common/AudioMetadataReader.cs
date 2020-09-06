// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Android.Media;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class AudioMetadataReader : IAudioMetadataReader
    {
        #region Fields
        private readonly ILogger logger;
        private readonly IAudioFileLoader audioFileLoader;
        #endregion

        #region Ctor
        public AudioMetadataReader(ILogger logger, IAudioFileLoader audioFileLoader)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.audioFileLoader = audioFileLoader ?? throw new ArgumentNullException(nameof(audioFileLoader));
        }
        #endregion

        #region Methods

        public async Task<IAudioMetaData> GetMetaDataAsync(Emka3.PracticeLooper.Model.Player.AudioSource audioSource)
        {
            try
            {
                var player = new MediaPlayer();
                player.Reset();
                var dataSource = audioFileLoader.GetAbsoluteFilePath(audioSource);
                player.SetDataSource(dataSource);
                player.Prepare();
                return await Task.FromResult(new AudioMetadata(TimeSpan.FromMilliseconds(player.Duration).TotalSeconds));
            }
            catch (Exception ex)
            {
                await logger?.LogErrorAsync(ex);
                throw;
            }
        }
        #endregion
    }
}
