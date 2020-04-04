// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.IO;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Emka.PracticeLooper.Mobile.Common
{
    [Preserve(AllMembers = true)]
    public class FilePicker : IFilePicker
    {
        private readonly IFileRepository fileRepository;
        private readonly IAudioMetadataReader audioMetadataReader;

        public FilePicker(IFileRepository fileRepository, IAudioMetadataReader audioMetadataReader)
        {
            this.fileRepository = fileRepository;
            this.audioMetadataReader = audioMetadataReader;
        }

        public async Task<AudioSource> ShowPicker()
        {
            AudioSource result = null;
            bool isIos = Device.RuntimePlatform == Device.iOS;

            string[] allowedTypes = isIos ? new string[] { "public.audio" } : new string[] { "audio/*" };

            FileData fileData = await CrossFilePicker.Current.PickFile(allowedTypes);

            if (fileData == null)
                return result; // user canceled file picking

            var path = await fileRepository.SaveFileAsync(fileData.FileName, fileData.DataArray);

            result = new AudioSource
            {
                FileName = Path.GetFileNameWithoutExtension(path),
                Source = path,
                Type = AudioSourceType.Local
            };

            // get duration
            var metadata = await audioMetadataReader.GetMetaDataAsync(result);

            result.Duration = metadata.Duration;

            return result;
        }
    }
}
