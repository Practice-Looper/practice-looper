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
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class AudioFileRepository : IFileRepository
    {
        #region Fields
        private readonly IConfigurationService configurationService;
        private readonly IDeviceStorageService deviceStorageService;
        #endregion

        #region Ctor

        public AudioFileRepository(IConfigurationService configurationService, IDeviceStorageService deviceStorageService)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.deviceStorageService = deviceStorageService ?? throw new ArgumentNullException(nameof(deviceStorageService));
        }
        #endregion

        #region Methods
        public async Task<AudioSourceType> SaveFileAsync(string fileName, byte[] data)
        {
            string targetPath = configurationService.GetValue(PreferenceKeys.InternalStoragePath);
            double fileSize = (data.Length / 1024d) / 1024d;
            var freeExternalStorage = deviceStorageService.GetAvailableExternalStorage();
            var freeInternalStorage = deviceStorageService.GetAvailableInternalStorage();
            var isNotEnoughInternalStorage = fileSize > freeInternalStorage;
            
            if (isNotEnoughInternalStorage)
            {
                var storageAccess = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                var mounted = false;
                if (storageAccess == PermissionStatus.Granted)
                {
                    mounted = Android.OS.Environment.ExternalStorageState == Android.OS.Environment.MediaMounted;
                }

                if (mounted)
                {
                    targetPath = await configurationService.GetValueAsync(PreferenceKeys.ExternalStoragePath);

                    if (fileSize > freeExternalStorage)
                    {
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }


            await Task.Run(() =>
            {
                File.WriteAllBytes(Path.Combine(targetPath, fileName), data);
            });

            var exists = File.Exists(Path.Combine(targetPath, fileName));

            if (exists && isNotEnoughInternalStorage)
            {
                return AudioSourceType.LocalExternal;
            }

            if (exists && !isNotEnoughInternalStorage)
            {
                return AudioSourceType.LocalInternal;
            }

            return AudioSourceType.None;  
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
