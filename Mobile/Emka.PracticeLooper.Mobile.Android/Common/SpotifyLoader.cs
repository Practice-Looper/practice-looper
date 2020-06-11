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
using Com.Spotify.Android.Appremote.Api.Error;
using Com.Spotify.Sdk.Android.Auth;
using Emka3.PracticeLooper.Model;
using Emka.PracticeLooper.Mobile.Common;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class SpotifyLoader : Java.Lang.Object, IConnectorConnectionListener, ISpotifyLoader
    {
        #region Fields
        static AutoResetEvent connectedEvent;
        static AutoResetEvent tokenEvent;
        private SpotifyAppRemote api;
        private string token;
        private readonly IConfigurationService configurationService;
        private readonly ILogger logger;
        private readonly IStringLocalizer stringLocalizer;
        #endregion

        #region Ctor

        public SpotifyLoader(ILogger logger, IStringLocalizer stringLocalizer)
        {
            configurationService = Factory.GetConfigService();
            this.logger = logger;
            this.stringLocalizer = stringLocalizer;
        }
        #endregion

        #region Events

        public event EventHandler Disconnected;
        #endregion

        #region Properties
        public object RemoteApi => api;

        public string Token
        {
            get => token; set
            {
                token = value;
                tokenEvent?.Set();
            }
        }

        public bool Authorized { get; private set; }
        #endregion

        #region Methods

        public bool Initialize(string songUri = "")
        {
            try
            {
                StartAuthorization();
                tokenEvent.WaitOne();

                StartConnection();
                connectedEvent.WaitOne();

                Authorized = api != null && api.IsConnected && !string.IsNullOrEmpty(token);
                return Authorized;
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex, new Dictionary<string, string>
                {
                     { "SpotifyClientId", configurationService.GetValue("SpotifyClientId") },
                     { "redirectUri", configurationService.GetValue("SpotifyClientRedirectUri") },
                     { "requestCode", configurationService.GetValue<int>("SpotifyClientRequestCode").ToString() },
                     { "scopes", configurationService.GetValue<string>("SpotifyClientScopes") }
                });
                throw;
            }
        }

        public async Task<bool> InitializeAsync(string songUri = "")
        {
            tokenEvent = new AutoResetEvent(false);
            connectedEvent = new AutoResetEvent(false);

            return await Task.Run(() => Initialize(songUri));
        }

        public void OnConnected(SpotifyAppRemote api)
        {
            this.api = api;
            connectedEvent.Set();
        }

        public void OnFailure(Throwable error)
        {
            logger?.LogError(new System.Exception(error.Message), new Dictionary<string, string>
                {
                     { "SpotifyClientId", configurationService.GetValue("SpotifyClientId") },
                     { "requestCode", configurationService.GetValue<int>("SpotifyClientRequestCode").ToString() }
                });
            if (error is NotLoggedInException || error is UserNotAuthorizedException)
            {
                StartAuthorization();
            }
            else if (error is CouldNotFindSpotifyApp)
            {
                AlertDialog.Builder alertDiag = new AlertDialog.Builder(GlobalApp.MainActivity);
                alertDiag.SetTitle(stringLocalizer.GetLocalizedString("Hint_Caption_SpotifyMissing"));
                alertDiag.SetMessage(stringLocalizer.GetLocalizedString("Hint_Content_SpotifyMissing"));
                alertDiag.SetPositiveButton(stringLocalizer.GetLocalizedString("Ok"), (senderAlert, args) =>
                {
                    AuthorizationClient.OpenDownloadSpotifyActivity(GlobalApp.MainActivity);
                    connectedEvent.Set();
                    tokenEvent.Set();
                });

                alertDiag.SetNegativeButton(stringLocalizer.GetLocalizedString("Cancel"), (senderAlert, args) =>
                {
                    alertDiag.Dispose();
                    connectedEvent.Set();
                    tokenEvent.Set();
                });

                Dialog diag = alertDiag.Create();
                diag.Show();
            }
            else if (error is SpotifyConnectionTerminatedException)
            {
                Authorized = false;
                api = null;
                Disconnected?.Invoke(this, new SpotifyDisconnectedEventArgs(false));
            }
            else
            {
                // todo: show error dialog!
                connectedEvent.Set();
                tokenEvent.Set();
                throw error;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (api != null && api.IsConnected)
                {
                    SpotifyAppRemote.Disconnect(api);
                    Authorized = false;
                }
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex);
            }
        }

        private void StartAuthorization()
        {
            var clientId = configurationService.GetValue("SpotifyClientId");
            var redirectUri = configurationService.GetValue("SpotifyClientRedirectUri");
            var requestCode = configurationService.GetValue<int>("SpotifyClientRequestCode");
            var scopes = configurationService.GetValue<string>("SpotifyClientScopes");

            AuthorizationRequest.Builder builder = new AuthorizationRequest.Builder(clientId, AuthorizationResponse.Type.Token, redirectUri);

            builder.SetScopes(scopes.Split(" "));
            var request = builder.Build();
            MainThread.BeginInvokeOnMainThread(() => AuthorizationClient.OpenLoginActivity(GlobalApp.MainActivity, Convert.ToInt32(requestCode), request));
        }

        private void StartConnection()
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
            MainThread.BeginInvokeOnMainThread(() => SpotifyAppRemote.Connect(Application.Context, connectionParams, this));
        }
        #endregion
    }
}
