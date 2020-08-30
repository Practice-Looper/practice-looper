// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.IO;
using System.Threading.Tasks;
using Android.Media;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class AudioMetadataReader : IAudioMetadataReader
    {
        #region Fields
        private readonly ILogger logger;
        private readonly IConfigurationService configurationService;
        #endregion

        #region Ctor
        public AudioMetadataReader(ILogger logger, IConfigurationService configurationService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }
        #endregion

        #region Methods

        public async Task<IAudioMetaData> GetMetaDataAsync(Emka3.PracticeLooper.Model.Player.AudioSource audioSource)
        {
            try
            {
                var player = new MediaPlayer();
                player.Reset();
                player.SetDataSource(Path.Combine(configurationService.GetValue(PreferenceKeys.InternalStoragePath), audioSource.Source));
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
