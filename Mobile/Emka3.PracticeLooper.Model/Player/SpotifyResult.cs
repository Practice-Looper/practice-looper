// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Emka3.PracticeLooper.Model.Player
{
    public class SpotifyResult
    {
        public SpotifyResult()
        {
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public List<SpotifyImage> Images { get; set; }

        [JsonIgnore]
        public string ImageSmall { get => Images.Any() ? Images?.OrderBy(i => i.Height).ToArray()[0]?.Url : string.Empty; }

        [JsonIgnore]
        public string ImageMiddle { get => Images.Any() ? Images?.OrderBy(i => i.Height).ToArray()[1]?.Url : string.Empty; }

        [JsonIgnore]
        public string ImageLarge { get => Images.Any() ? Images?.OrderBy(i => i.Height).ToArray()[2]?.Url : string.Empty; }
    }
}
