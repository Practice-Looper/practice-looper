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

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class SpotifyLoader : Java.Lang.Object, IConnectorConnectionListener, ISpotifyLoader
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
            autoResetEvent = new AutoResetEvent(false);
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
                var clientId = configurationService.GetValue("SpotifyClientId");
                var redirectUri = configurationService.GetValue("SpotifyClientRedirectUri");
                var requestCode = configurationService.GetValue<int>("SpotifyClientRequestCode");
                var scopes = configurationService.GetValue<string>("SpotifyClientScopes");

                ConnectionParams connectionParams = new ConnectionParams
                .Builder(clientId)
                .SetRedirectUri(redirectUri)
                .ShowAuthView(true)
                .Build();
                MainThread.BeginInvokeOnMainThread(()=> SpotifyAppRemote.Connect(Application.Context, connectionParams, this));


                Com.Spotify.Sdk.Android.Auth.AuthorizationRequest.Builder builder =
        new Com.Spotify.Sdk.Android.Auth.AuthorizationRequest.Builder(clientId, Com.Spotify.Sdk.Android.Auth.AuthorizationResponse.Type.Token, redirectUri);

                builder.SetScopes(scopes.Split(" "));
                Com.Spotify.Sdk.Android.Auth.AuthorizationRequest request = builder.Build();
                Com.Spotify.Sdk.Android.Auth.AuthorizationClient.OpenLoginActivity(GlobalApp.MainActivity, Convert.ToInt32(requestCode), request);

                autoResetEvent.WaitOne();
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
