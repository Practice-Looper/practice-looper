// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading.Tasks;

namespace Emka3.PracticeLooper.Services.Contracts.Rest
{
    public interface ISpotifyApiService
    {
        Task SearchForTerm(string term);
    }
}
