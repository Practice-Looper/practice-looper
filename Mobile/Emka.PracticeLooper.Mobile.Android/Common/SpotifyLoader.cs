// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Com.Spotify.Android.Appremote.Api;
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
using Emka3.PracticeLooper.Config.Contracts;
using Android.Content.PM;
using Emka3.PracticeLooper.Model.Player;

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
        private readonly ITokenStorage tokenStorage;
        #endregion

        #region Ctor

        public SpotifyLoader(ILogger logger,
            IConfigurationService configurationService,
            ITokenStorage tokenStorage)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.tokenStorage = tokenStorage ?? throw new ArgumentNullException(nameof(tokenStorage));
        }
        #endregion

        #region Events
        public event EventHandler<AudioSourceType> WebAuthorizationRequested;
        public event EventHandler Disconnected;
        #endregion

        #region Properties
        public OAuthAuthenticator Authenticator { get; private set; }
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
                var connectionTimeout = configurationService.GetValue<int>("SpotifyConnectionTimeOut");

                StartAuthorization();
                tokenEvent.WaitOne(TimeSpan.FromSeconds(connectionTimeout));

                StartConnection();
                connectedEvent.WaitOne(TimeSpan.FromSeconds(connectionTimeout));

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
            Authorized = api != null && api.IsConnected && !string.IsNullOrEmpty(token);
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
            else if (error is SpotifyConnectionTerminatedException)
            {
                Authorized = false;
                api = null;
                Disconnected?.Invoke(this, new SpotifyDisconnectedEventArgs(false));
            }
            else if (error is CouldNotFindSpotifyApp)
            {

            }
            else
            {
                // todo: show error dialog!
                connectedEvent.Set();
                tokenEvent.Set();
                logger.LogError(new System.Exception($"UNEXPECTED SPOTIFY LOADER ERROR {error?.Message}"));
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
            var isSpotifyInstalled = IsSpotifyInstalled();

            if (isSpotifyInstalled)
            {
                var clientId = configurationService.GetValue("SpotifyClientId");
                var redirectUri = configurationService.GetValue("SpotifyClientRedirectUri");
                var requestCode = configurationService.GetValue<int>("SpotifyClientRequestCode");
                var scopes = configurationService.GetValue<string>("SpotifyClientScopes");

                AuthorizationRequest.Builder builder = new AuthorizationRequest.Builder(clientId, Com.Spotify.Sdk.Android.Auth.AuthorizationResponse.Type.Token, redirectUri);

                builder.SetScopes(scopes.Split(" "));
                var request = builder.Build();
                MainThread.BeginInvokeOnMainThread(() => AuthorizationClient.OpenLoginActivity(GlobalApp.MainActivity, Convert.ToInt32(requestCode), request));
            }
            else
            {
                OnWebAuthorizationRequested();
            }
        }

        private void StartConnection()
        {
            if (IsSpotifyInstalled())
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
                return;
            }

            connectedEvent.Set();
        }

        public void InstallSpotify()
        {
            AuthorizationClient.OpenDownloadSpotifyActivity(GlobalApp.MainActivity);
        }

        public bool IsSpotifyInstalled()
        {
            try
            {
                Application.Context.PackageManager.GetPackageInfo("com.spotify.music", PackageInfoFlags.Activities);
                return true;
            }
            catch (PackageManager.NameNotFoundException)
            {
                return false;
            }
        }

        private string GetRandomCoumarinSong()
        {
            string result = string.Empty;
            try
            {
                var tracks = new[]
                {
                    "spotify:track:5EclxNYgNaP921FqhGsiHd", // dreams
                    "spotify:track:4nDaTStabiIs8Re1FYcipd", // make it right
                    "spotify:track:7qfNL2fbqK3vBYxEm9WVVj", // mirage
                    "spotify:track:1dv8jyahW3Xf2lxwKCrpSl"  // never again
                };

                var random = new Random();
                var randomIndex = random.Next(0, tracks.Length - 1);

                result = tracks[randomIndex];
            }
            catch (System.Exception ex)
            {
                logger?.LogError(ex);
            }

            return result;
        }

        public OAuthAuthenticator GetAuthenticator()
        {
            var id = configurationService.GetValue("SpotifyClientId");
            var secret = configurationService.GetValue("SpotifyClientSecret");
            var scopes = configurationService.GetValue("SpotifyClientScopes");
            var callbackUri = configurationService.GetValue("SpotifyClientRedirectUri");
            var tokenUri = configurationService.GetValue("SpotifyClientTokenUri");
            var authUri = configurationService.GetValue("SpotifyClientAuthUri");

            Authenticator = new OAuthAuthenticator(id, secret, scopes, new Uri(authUri), new Uri(callbackUri), new Uri(tokenUri), null, true);
            Authenticator.Completed += async (s, e) =>
            {
                Authorized = e.IsAuthenticated;
                if (e.IsAuthenticated)
                {
                    token = e.Account.Properties["access_token"];
                    var expirationTime = e.Account.Properties["expires_in"];
                    tokenStorage.UpdateAccessToken(AudioSourceType.Spotify, token, int.Parse(expirationTime));
                    var refreshToken = e.Account.Properties["refresh_token"];
                    await tokenStorage.UpdateRefreshTokenAsync(AudioSourceType.Spotify, refreshToken);
                }

                tokenEvent.Set();
                connectedEvent.Set();
            };

            return Authenticator;
        }

        private async void OnWebAuthorizationRequested()
        {
            Authorized = await tokenStorage.HasRefreshToken(AudioSourceType.Spotify);
            if (!Authorized)
            {
                WebAuthorizationRequested.Invoke(this, AudioSourceType.Spotify);
            }
            else
            {
                tokenEvent.Set();
            }
        }
        #endregion
    }
}
