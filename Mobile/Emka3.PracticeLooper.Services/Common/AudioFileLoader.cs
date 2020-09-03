// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.IO;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;

namespace Emka3.PracticeLooper.Services.Common
{
    public class AudioFileLoader: IAudioFileLoader
    {
        private readonly IConfigurationService configurationService;

        public AudioFileLoader(IConfigurationService configurationService)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }

        public string GetAbsoluteFilePath(AudioSource audioSource)
        {
            switch (audioSource.Type)
            {
                case AudioSourceType.Spotify:
                case AudioSourceType.None:
                    return string.Empty;
                case AudioSourceType.LocalInternal:
                    var internalPath = Path.Combine(configurationService.GetValue(PreferenceKeys.InternalStoragePath), audioSource.Source);
                    return internalPath;
                case AudioSourceType.LocalExternal:
                    var externalPath = Path.Combine(configurationService.GetValue(PreferenceKeys.ExternalStoragePath), audioSource.Source);
                    return externalPath;
                default:
                    throw new ArgumentException($"invalid audio source type {audioSource.Type}");
            }
        }

        public Stream GetFileStream(AudioSource audioSource)
        {
            switch (audioSource.Type)
            {
                case AudioSourceType.Spotify:
                case AudioSourceType.None:
                    return FileStream.Null;
                case AudioSourceType.LocalInternal:
                    var internalStream = new FileStream(Path.Combine(configurationService.GetValue(PreferenceKeys.InternalStoragePath), audioSource.Source), FileMode.Open, FileAccess.Read);
                    return internalStream;
                case AudioSourceType.LocalExternal:
                    var externalStream = new FileStream(Path.Combine(configurationService.GetValue(PreferenceKeys.ExternalStoragePath), audioSource.Source), FileMode.Open, FileAccess.Read);
                    return externalStream;
                default:
                    throw new ArgumentException($"invalid audio source type {audioSource.Type}");
            }
        }
    }
}
