// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Services.Contracts.Rest
{
    [Preserve(AllMembers = true)]
    public interface ISpotifyApiService
    {
        bool UserPremiumCheckSuccessful { get; }

        Task<bool> PlayTrack(string trackId, int positionMs);
        Task<List<SpotifyTrack>> SearchTrackByName(string term, CancellationToken cancellationToken);
        Task<Tuple<HttpStatusCode, bool>> IsPremiumUser();
        Task<bool> PauseCurrentPlayback();
        Task<bool> SeekTo(long position);
        Task<List<SpotifyDevice>> GetActiveDevices();
        Task<double> GetCurrentPlaybackPosition();
        double GetAverageRequestTime();
    }
}
