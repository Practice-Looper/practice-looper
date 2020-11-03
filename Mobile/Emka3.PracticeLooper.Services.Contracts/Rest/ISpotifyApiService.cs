﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;

namespace Emka3.PracticeLooper.Services.Contracts.Rest
{
    public interface ISpotifyApiService
    {
       

        Task<List<SpotifyTrack>> SearchTrackByName(string term, CancellationToken cancellationToken);

        bool UserPremiumCheckSuccessful { get; }

        Task<Tuple<HttpStatusCode, bool>> IsPremiumUser();

        Task PauseCurrentPlayback();
    }
}
