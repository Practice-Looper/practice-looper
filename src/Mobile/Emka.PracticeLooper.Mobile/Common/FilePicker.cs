// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.IO;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Foundation;
using MobileCoreServices;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Common
{
    public class FilePicker : IFilePicker
    {
        private readonly IFileRepository fileRepository;

        public FilePicker(IFileRepository fileRepository)
        {
            this.fileRepository = fileRepository;
        }

        public async Task<AudioSource> ShowPicker()
        {
            AudioSource result = null;
            try
            {
                string[] allowedTypes = null;
                if (Device.RuntimePlatform == Device.iOS)
                {
                    allowedTypes = new string[] { UTType.Audio.ToString() };
                }
                else if (Device.RuntimePlatform == Device.iOS)
                {
                    allowedTypes = new string[] { "audio/*" };
                }

                FileData fileData = await CrossFilePicker.Current.PickFile(allowedTypes).ConfigureAwait(false);
                var path = await fileRepository.SaveFileAsync(fileData.FileName, fileData.DataArray);

                if (fileData == null)
                    return result; // user canceled file picking

                result = new AudioSource
                {
                    FileName = Path.GetFileNameWithoutExtension(path),
                    Source = path
                };

            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }
    }
}
