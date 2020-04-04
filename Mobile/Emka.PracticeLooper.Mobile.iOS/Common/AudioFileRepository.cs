// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.IO;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Foundation;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    [Preserve(AllMembers = true)]
    public class AudioFileRepository : IFileRepository
    {
        #region Fields
        private bool hasCloudAccess;
        #endregion

        #region Ctor
        public AudioFileRepository()
        {
            hasCloudAccess = false;
        }
        #endregion

        #region Methods
        public async Task<string> SaveFileAsync(string fileName, byte[] data)
        {
            var url = await CheckCloudAccess();
            string targetPath = string.Empty;
            hasCloudAccess = url != null;

            if (hasCloudAccess)
            {
                targetPath = Path.Combine(url.Path, "Documents");
                await EnsureDocumentsPath(Path.Combine(url.Path, "Documents"));

            }
            else
            {
                targetPath = GlobalApp.ConfigurationService.LocalPath;
            }

            await Task.Run(() =>
            {
                File.WriteAllBytes(Path.Combine(targetPath, fileName), data);
            });

            return Path.Combine(targetPath, fileName);
        }

        private async Task<NSUrl> CheckCloudAccess()
        {
            var cloudTask = Task.Run(() =>
            {
                return NSFileManager.DefaultManager.GetUrlForUbiquityContainer(null);
            });

            return await cloudTask;
        }

        private async Task EnsureDocumentsPath(string path)
        {
            await Task.Run(() =>
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            });
        }


        public async Task DeleteFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (File.Exists(fileName))
            {
                await Task.Run(() =>
                {
                    File.Delete(fileName);
                });
            }
        }
        #endregion Methods
    }
}
