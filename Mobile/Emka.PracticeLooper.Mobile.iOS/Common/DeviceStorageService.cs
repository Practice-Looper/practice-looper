// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Foundation;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    [Preserve(AllMembers = true)]
    public class DeviceStorageService : IDeviceStorageService
    {
        public long GetAvailableExternalStorage()
        {
            // no external storage on iOS
            return 0;
        }

        public long GetAvailableInternalStorage()
        {
            ulong availableStorage = NSFileManager.DefaultManager.GetFileSystemAttributes(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).FreeSize / (1024 * 1024);
            return (long)availableStorage;    
        }
    }
}
