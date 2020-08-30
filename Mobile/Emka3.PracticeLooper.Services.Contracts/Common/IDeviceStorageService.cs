﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    public interface IDeviceStorageService
    {
        long GetAvailableInternalStorage();
        long GetAvailableExternalStorage();
    }
}
