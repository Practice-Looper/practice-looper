// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Model.Player
{
    [Preserve(AllMembers = true)]
    public class SpotifyImage
    {
        public string Url { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}