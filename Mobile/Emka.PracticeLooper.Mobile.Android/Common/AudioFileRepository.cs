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

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class AudioFileRepository : IFileRepository
    {
        #region Fields
        private readonly IConfigurationService configurationService;
        private readonly IDeviceStorageService deviceStorageService;
        private readonly IPermissionsManager permissionsManager;
        #endregion

        #region Ctor

        public AudioFileRepository(IConfigurationService configurationService, IDeviceStorageService deviceStorageService, IPermissionsManager permissionsManager)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.deviceStorageService = deviceStorageService ?? throw new ArgumentNullException(nameof(deviceStorageService));
            this.permissionsManager = permissionsManager ?? throw new ArgumentNullException(nameof(permissionsManager));
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
                var storageAccess = await permissionsManager.CheckStorageWritePermissionAsync();

                if (storageAccess)
                {
                    var isNotEnoughExternalStorage = fileSize > freeExternalStorage;

                    if (isNotEnoughExternalStorage)
                    {
                        throw new InvalidOperationException("Not enough external space left.");
                    }

                    targetPath = await configurationService.GetValueAsync(PreferenceKeys.ExternalStoragePath) ?? throw new InvalidOperationException();
                }
                else
                {
                    throw new InvalidOperationException("Not enough internal space left.");
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

            var path = string.Empty; 

            if (File.Exists(Path.Combine(configurationService.GetValue<string>(PreferenceKeys.InternalStoragePath), fileName)))
            {
                path = Path.Combine(configurationService.GetValue<string>(PreferenceKeys.InternalStoragePath), fileName);
            }

            if (File.Exists(Path.Combine(configurationService.GetValue<string>(PreferenceKeys.ExternalStoragePath), fileName)))
            {
                path = Path.Combine(configurationService.GetValue<string>(PreferenceKeys.ExternalStoragePath), fileName);
            }

            if (path != string.Empty)
            {
                await Task.Run(() =>
                {
                    File.Delete(path);
                });
            }
            
        }
        #endregion Methods
    }
}
