// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka3.PracticeLooper.Services.Contracts.Common;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class DeviceStorageService : IDeviceStorageService
    {
        public long GetAvailableExternalStorage()
        {
            var availableExternalDeviceStorage = Android.OS.Environment.ExternalStorageDirectory.UsableSpace;
            return availableExternalDeviceStorage;
        }

        public long GetAvailableInternalStorage()
        {
            var availableInternalDeviceStorage = Android.OS.Environment.RootDirectory.UsableSpace;
            return availableInternalDeviceStorage;
        }
    }
}
