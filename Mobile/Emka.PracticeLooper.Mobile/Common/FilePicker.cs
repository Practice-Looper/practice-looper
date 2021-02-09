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
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Essentials;
using System.Collections.Generic;

namespace Emka.PracticeLooper.Mobile.Common
{
    [Preserve(AllMembers = true)]
    public class FilePicker : IFilePicker
    {
        private readonly IFileRepository fileRepository;
        private readonly IAudioMetadataReader audioMetadataReader;
        private readonly IPermissionsManager permissionsManager;

        public FilePicker(IFileRepository fileRepository, IAudioMetadataReader audioMetadataReader, IPermissionsManager permissionsManager)
        {
            this.fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            this.audioMetadataReader = audioMetadataReader ?? throw new ArgumentNullException(nameof(audioMetadataReader));
            this.permissionsManager = permissionsManager ?? throw new ArgumentNullException(nameof(permissionsManager));
        }

        public async Task<AudioSource> ShowPicker()
        {
            AudioSource result = null;
            bool isIos = Device.RuntimePlatform == Device.iOS;

            var storageAccess = await permissionsManager.CheckStorageWritePermissionAsync();

            if (!storageAccess)
            {
                await permissionsManager.RequestStorageWritePermissionAsync();
            }

            var customFileType =
                new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.audio" } },
                    { DevicePlatform.Android, new[] { "audio/*" } }
                });

            var options = new PickOptions
            {
                FileTypes = customFileType,
            };

            FileResult fileData = await Xamarin.Essentials.FilePicker.PickAsync(options);

            if (fileData == null)
                return result; // user canceled file picking

            var stream = await fileData.OpenReadAsync();
            var bytesInStream = new byte[stream.Length];
            stream.Read(bytesInStream, 0, bytesInStream.Length);

            var type = await fileRepository.SaveFileAsync(fileData.FileName, bytesInStream);

            if (type == AudioSourceType.None)
            {
                throw new InvalidOperationException("unexpected error while saving file");
            }

            result = new AudioSource
            {
                FileName = Path.GetFileNameWithoutExtension(fileData.FileName),
                Source = fileData.FileName,
                Type = type
            };

            // get duration
            var metadata = await audioMetadataReader.GetMetaDataAsync(result);
            result.Duration = metadata.Duration;

            return result;
        }
    }
}
