// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.IO;
using Emka3.PracticeLooper.Model.Player;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    public interface IAudioFileLoader
    {
        string GetAbsoluteFilePath(AudioSource audioSource);
        Stream GetFileStream(AudioSource audioSource);
    }
}
