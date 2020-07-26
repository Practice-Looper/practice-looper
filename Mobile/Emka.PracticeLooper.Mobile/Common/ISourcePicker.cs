﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;

namespace Emka.PracticeLooper.Mobile.Common
{
    public interface ISourcePicker : IFeature
    {
        Task<AudioSourceType> SelectFileSource();
    }
}
