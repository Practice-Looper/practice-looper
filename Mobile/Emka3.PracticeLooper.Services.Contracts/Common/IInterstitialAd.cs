﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    public interface IInterstitialAd
    {
        Task ShowAdAsync();
        void ShowAd();
    }
}
