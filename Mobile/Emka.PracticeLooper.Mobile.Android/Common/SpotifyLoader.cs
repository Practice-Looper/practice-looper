// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Com.Spotify.Android.Appremote.Api;
using Emka3.PracticeLooper.Config;
using Android.App;
using Java.Lang;
using System.Threading;
using Xamarin.Essentials;
using System;
using System.Collections.Generic;
using Emka3.PracticeLooper.Services.Contracts.Common;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class SpotifyLoader : Java.Lang.Object, IConnectorConnectionListener, ISpotifyLoader
    {
        #region Fields
        static AutoResetEvent autoResetEvent;
        private SpotifyAppRemote api;
        private readonly IConfigurationService configurationService;
        private readonly ILogger logger;
        #endregion

        #region Ctor

        public SpotifyLoader(ILogger logger)
        {
            configurationService = Factory.GetConfigService();
            autoResetEvent = new AutoResetEvent(false);
            this.logger = logger;
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
            var clientId = configurationService.GetValue("SpotifyClientId");
            var redirectUri = configurationService.GetValue("SpotifyClientRedirectUri");
            var requestCode = configurationService.GetValue<int>("SpotifyClientRequestCode");
            var scopes = configurationService.GetValue<string>("SpotifyClientScopes");

            try
            {
                ConnectionParams connectionParams = new ConnectionParams
                .Builder(clientId)
                .SetRedirectUri(redirectUri)
                .ShowAuthView(true)
                .Build();
                MainThread.BeginInvokeOnMainThread(() => SpotifyAppRemote.Connect(Application.Context, connectionParams, this));

                Com.Spotify.Sdk.Android.Auth.AuthorizationRequest.Builder builder =
        new Com.Spotify.Sdk.Android.Auth.AuthorizationRequest.Builder(clientId, Com.Spotify.Sdk.Android.Auth.AuthorizationResponse.Type.Token, redirectUri);

                builder.SetScopes(scopes.Split(" "));
                Com.Spotify.Sdk.Android.Auth.AuthorizationRequest request = builder.Build();
                Com.Spotify.Sdk.Android.Auth.AuthorizationClient.OpenLoginActivity(GlobalApp.MainActivity, Convert.ToInt32(requestCode), request);

                autoResetEvent.WaitOne();
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex, new Dictionary<string, string>
                {
                     { "SpotifyClientId", clientId },
                     { "redirectUri", redirectUri },
                     { "requestCode", requestCode.ToString() },
                     { "scopes", scopes }
                });
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
            logger?.LogError(new System.Exception(error.Message), new Dictionary<string, string>
                {
                     { "SpotifyClientId", configurationService.GetValue("SpotifyClientId") },
                     { "requestCode", configurationService.GetValue<int>("SpotifyClientRequestCode").ToString() }
                });
            api = null;
            Authorized = false;
            autoResetEvent.Set();
            // todo: check if user needs to login
        }

        public void Disconnect()
        {
            try
            {
                if (api != null && api.IsConnected)
                {
                    SpotifyAppRemote.Disconnect(api);
                }
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex);
            }
        }
        #endregion
    }
}
