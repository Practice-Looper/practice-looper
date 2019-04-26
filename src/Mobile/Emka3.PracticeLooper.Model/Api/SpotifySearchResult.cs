// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;

namespace Emka3.PracticeLooper.Model.Api
{
    public class SpotifySearchResult
    {
        public SpotifySearchResult()
        {
        }

        public SpotifyAlbumRresult Albums { get; set; }
        public SpotifyArtistResult Artists { get; set; }
        public SpotifyTrackResult Tracks { get; set; }
    }
}
