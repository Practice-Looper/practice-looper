// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.IO;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;

namespace Emka.PracticeLooper.Mobile.Common
{
    public class FilePicker : IFilePicker
    {
        public async Task<FileAudioSource> ShowPicker()
        {
            FileAudioSource result = null;
            try
            {
                FileData fileData = await CrossFilePicker.Current.PickFile().ConfigureAwait(false);
                if (fileData == null)
                    return result; // user canceled file picking

                result = new FileAudioSource(fileData.FilePath)
                {
                    FileName = Path.GetFileNameWithoutExtension(fileData.FileName)
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
