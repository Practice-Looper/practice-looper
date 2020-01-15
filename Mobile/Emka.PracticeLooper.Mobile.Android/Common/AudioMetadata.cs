// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using Emka3.PracticeLooper.Model.Player;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class AudioMetadata : IAudioMetaData
    {
        public AudioMetadata(double duration)
        {
            Duration = duration;
        }

        public double Duration { get; private set; }
    }
}
