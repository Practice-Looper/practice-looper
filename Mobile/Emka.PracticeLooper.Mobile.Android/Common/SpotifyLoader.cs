// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Com.Spotify.Android.Appremote.Api;
using Emka3.PracticeLooper.Config;
using Android.App;
using Java.Lang;
using System.Threading;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class SpotifyLoader : Object, IConnectorConnectionListener, ISpotifyLoader
    {
        #region Fields
        static AutoResetEvent autoResetEvent;
        private SpotifyAppRemote api;
        private readonly IConfigurationService configurationService;
        #endregion

        #region Ctor

        public SpotifyLoader()
        {
            configurationService = Factory.GetConfigService();
        }
        #endregion

        #region Properties

        public object RemoteApi => api;

        public string Token { get; set; }

        public bool Authorized { get; private set; }
        #endregion

        #region Methods

        public void Initialize(string songUri = "")
        {
            try
            {
                autoResetEvent = new AutoResetEvent(false);
                var clientId = configurationService.GetValue("auth:spotify:client:id");
                var redirectUri = configurationService.GetValue("auth:spotify:client:uri:redirect");

                ConnectionParams connectionParams = new ConnectionParams
                .Builder(clientId)
                .SetRedirectUri(redirectUri)
                .ShowAuthView(true)
                .Build();

                SpotifyAppRemote.Connect(Application.Context, connectionParams, this);

                autoResetEvent.WaitOne(2000);
            }
            catch (System.Exception)
            {
                // todo: log
                throw;
            }
        }

        public async Task<bool> InitializeAsync(string songUri = "")
        {
            await Task.Run(() => Initialize(songUri));
            return true;
        }

        public void OnConnected(SpotifyAppRemote api)
        {
            this.api = api;
            Authorized = api.IsConnected;
            autoResetEvent.Set();
        }

        public void OnFailure(Throwable error)
        {
            this.api = null;
            Authorized = false;
            autoResetEvent.Set();
            // todo: log
            // todo: check if user needs to login
        }
        #endregion
    }
}
