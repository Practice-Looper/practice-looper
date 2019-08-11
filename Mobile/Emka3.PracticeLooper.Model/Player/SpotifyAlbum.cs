// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Emka3.PracticeLooper.Model.Player
{
    public class SpotifyAlbum : SpotifyEntity
    {
        public List<SpotifyImage> Images { get; set; }
        public List<SpotifyArtist> Artists { get; set; }
        
        [JsonIgnore]
        public string ImageSmall { get => Images != null && Images.Any() ? Images?.OrderBy(i => i.Height).ToArray()[0]?.Url : string.Empty; }

        [JsonIgnore]
        public string ImageMiddle { get => Images != null && Images.Any() ? Images?.OrderBy(i => i.Height).ToArray()[1]?.Url : string.Empty; }

        [JsonIgnore]
        public string ImageLarge { get => Images != null && Images.Any() ? Images?.OrderBy(i => i.Height).ToArray()[2]?.Url : string.Empty; }
    }
}
