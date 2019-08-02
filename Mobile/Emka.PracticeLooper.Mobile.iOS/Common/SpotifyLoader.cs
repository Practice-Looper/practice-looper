// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using SpotifyBindings.iOS;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class SpotifyLoader : ISpotifyLoader
    {
        private readonly SPTAppRemote api;
        public SpotifyLoader()
        {
            api = GlobalApp.SPTRemoteApi;
        }

        public async Task<bool> Initialize()
        {
            return await Task.Run(() =>
            {
                bool result = false;
                if (GlobalApp.ConfigurationService.IsSpotifyInstalled)
                {
                    SPTAppRemote.CheckIfSpotifyAppIsActive(async (isActive) =>
                    {
                        //if (!isActive)
                        //{
                            result = api.AuthorizeAndPlayURI(string.Empty);

                            // wait until spotify access token arrives!
                            await GlobalApp.SpotifyTokenCompletionSource.Task;
                        //}
                    });
                }

                return result;
            });
        }
    }
}
