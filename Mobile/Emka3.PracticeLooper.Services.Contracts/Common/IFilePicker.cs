// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    public interface IFilePicker
    {
        event EventHandler<AudioSource> SourceSelected;
        void ShowPicker();
        //Task<AudioSource> ShowPicker();
    }
}
