// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Linq;
using Newtonsoft.Json;

namespace Emka3.PracticeLooper.Model.Player
{
    public class SpotifyTrack : SpotifyEntity
    {
        public SpotifyAlbum Album { get; set; }

        [JsonProperty("duration_ms")]
        public double Duration { get; set; }

        public string Uri { get; set; }
        
        [JsonIgnore]
        public string ArtistNames => string.Join(", ", Album.Artists.Select(a => a.Name).ToList());
    }
}
