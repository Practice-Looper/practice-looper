// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Foundation;
using SpotifyBindings.iOS;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class SpotifyLoader : ISpotifyLoader
    {
        #region Fields

        private SPTAppRemote api;
        private readonly IConfigurationService configurationService;
        #endregion

        #region Ctor

        public SpotifyLoader()
        {
            this.configurationService = Factory.GetConfigService();
        }
        #endregion

        #region Properties

        public object RemoteApi => api;

        public string Token { get; set; }
        #endregion

        #region Methods

        public void Initialize()
        {
            var clientId = configurationService.GetValue("auth:spotify:client:id");
            var redirectUri = configurationService.GetValue("auth:spotify:client:uri:redirect");

            var appConfig = new SPTConfiguration(clientId, NSUrl.FromString(redirectUri));
            api = new SPTAppRemote(appConfig, SPTAppRemoteLogLevel.Info);

            if (GlobalApp.ConfigurationService.IsSpotifyInstalled)
            {
                SPTAppRemote.CheckIfSpotifyAppIsActive((isActive) =>
                {
                    if (!isActive)
                    {
                        // prompt user to authorize sloopy   
                    }

                    api.AuthorizeAndPlayURI(string.Empty);
                });
            }
            else
            {
                // prompt to install
            }
        }

        public async Task<bool> InitializeAsync()
        {
            bool result = false;
            if (GlobalApp.ConfigurationService.IsSpotifyInstalled)
            {
                SPTAppRemote.CheckIfSpotifyAppIsActive((isActive) =>
                {
                    //if (!isActive)
                    //{
                    result = api.AuthorizeAndPlayURI(string.Empty);
                    //}
                });

                // wait until spotify access token arrives!
                await GlobalApp.SpotifyTokenCompletionSource.Task;
            }

            return result;
        }
        #endregion
    }
}
