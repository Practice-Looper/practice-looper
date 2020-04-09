// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Threading.Tasks;

namespace Emka3.PracticeLooper.Services.Contracts.Player
{
    public interface ISpotifyLoader : IPlayerLoader
    {
        object RemoteApi { get; }
        string Token { get; set; }
        bool Authorized { get; }
        Task<bool> InitializeAsync(string songUri = "");
        bool Initialize(string songUri = "");
        void Disconnect();
    }
}
