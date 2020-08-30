// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.IO;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Foundation;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    [Preserve(AllMembers = true)]
    public class AudioFileRepository : IFileRepository
    {
        #region Fields
        private bool hasCloudAccess;
        private readonly IConfigurationService configurationService;
        private readonly IDeviceStorageService deviceStorageService;
        #endregion

        #region Ctor
        public AudioFileRepository(IConfigurationService configurationService, IDeviceStorageService deviceStorageService)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.deviceStorageService = deviceStorageService ?? throw new ArgumentNullException(nameof(deviceStorageService));
            hasCloudAccess = false;
        }
        #endregion

        #region Methods
        public async Task<AudioSourceType> SaveFileAsync(string fileName, byte[] data)
        {
            var url = await CheckCloudAccess();
            string targetPath = string.Empty;
            // don't use cloud storage for now
            hasCloudAccess = false;

            if (hasCloudAccess)
            {
                targetPath = Path.Combine(url.Path, "Documents");
                await EnsureDocumentsPath(Path.Combine(url.Path, "Documents"));
            }
            else
            {
                targetPath = configurationService.GetValue(PreferenceKeys.InternalStoragePath);
            }

            var freeDiskSpace = deviceStorageService.GetAvailableInternalStorage();
            double fileSize = (data.Length / 1024d) / 1024d;

            if (fileSize > freeDiskSpace)
            {
                return AudioSourceType.None;
            }

            await Task.Run(() =>
            {
                File.WriteAllBytes(Path.Combine(targetPath, fileName), data);
            });

            var exists = File.Exists(Path.Combine(targetPath, fileName));
            return exists ? AudioSourceType.LocalInternal : AudioSourceType.None;
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
