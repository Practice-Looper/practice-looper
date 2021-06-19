// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2021
using System.Collections.Generic;
using System.Linq;
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Model.Player
{
    [Preserve(AllMembers = true)]
    public class SpotifyTrackDetails
    {
        public SpotifyTrackDetails(string artist, string title, List<SpotifyImage> images)
        {
            Artist = artist;
            Title = title;
            Images = images;
        }

        public string Artist { get; }
        public string Title { get; }
        public List<SpotifyImage> Images { get; set; }
        public string ImageSmall { get => Images != null && Images.Any() ? Images?.OrderBy(i => i.Height).ToArray()[0]?.Url : string.Empty; }
        public string ImageMiddle { get => Images != null && Images.Any() ? Images?.OrderBy(i => i.Height).ToArray()[1]?.Url : string.Empty; }
        public string ImageLarge { get => Images != null && Images.Any() ? Images?.OrderBy(i => i.Height).ToArray()[2]?.Url : string.Empty; }
    }
}
