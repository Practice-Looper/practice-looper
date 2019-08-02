// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.IO;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class AudioFileRepository : IFileRepository
    {
        #region Methods
        public async Task<string> SaveFileAsync(string fileName, byte[] data)
        {
            try
            {
                //string targetPath = GlobalApp.ConfigurationService.LocalPath;
                //await Task.Run(() =>
                //{
                //    File.WriteAllBytes(Path.Combine(targetPath, fileName), data);
                //});

                return await Task.Run(() => fileName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            try
            {
                if (File.Exists(fileName))
                {
                    await Task.Run(() =>
                    {
                        File.Delete(fileName);
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion Methods
    }
}
