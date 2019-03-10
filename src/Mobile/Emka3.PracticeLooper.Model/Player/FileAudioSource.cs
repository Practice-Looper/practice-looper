// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
namespace Emka3.PracticeLooper.Model.Player
{
    public class FileAudioSource : IAudioSource
    {
        public FileAudioSource(string source)
        {
            this.Source = source;
        }

        public string Source { get; set; }
    }
}
