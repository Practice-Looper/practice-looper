// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;

namespace Emka3.PracticeLooper.Model.Player
{
    [Utils.Preserve(AllMembers = true)]
    [Flags]
    public enum AudioSourceType
    {
        None = 0,
        LocalInternal = 1,
        LocalExternal = 2,
        Spotify = 4
    }
}
