// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;
using SpotifyBindings.iOS;
using StoreKit;
using UIKit;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    [Preserve(AllMembers = true)]
    public class SpotifyLoader : SPTAppRemoteDelegate, ISpotifyLoader, ISKStoreProductViewControllerDelegate
    {
        #region Fields
        private TaskCompletionSource<AuthorizationResponse> authorizationCompletionSource;
        private SPTAppRemote api;
        static AutoResetEvent tokenEvent;
        static AutoResetEvent connectedEvent;
        private string token;
        private readonly IConfigurationService configurationService;
        private readonly ITokenStorage tokenStorage;
        private readonly ILogger logger;
        #endregion

        #region Ctor

        public SpotifyLoader(ILogger logger,
            IConfigurationService configurationService,
            ITokenStorage tokenStorage)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.tokenStorage = tokenStorage;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.tokenStorage = tokenStorage ?? throw new ArgumentNullException(nameof(tokenStorage));
        }
        #endregion

        #region Properties
        public OAuthAuthenticator Authenticator { get; private set; }
        public object RemoteApi => api;

        public string Token
        {
            get => token;

            set
            {
                token = value;
                if (tokenEvent != null)
                {
                    try
                    {
                        tokenEvent?.Set();
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex);
                        throw;
                    }
                }
            }
        }

        public bool Authorized { get; private set; }
        #endregion

        #region Events
        public event EventHandler<AudioSourceType> WebAuthorizationRequested;
        public event EventHandler Disconnected;
        #endregion

        #region Methods

        public bool Initialize(string songUri = "")
        {
            try
            {
                var connectionTimeout = configurationService.GetValue<int>("SpotifyConnectionTimeOut");

                if (string.IsNullOrEmpty(songUri))
                {
                    songUri = GetRandomCoumarinSong();
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StartAuthorization(songUri);
                });

                tokenEvent?.WaitOne(TimeSpan.FromSeconds(connectionTimeout));

                MainThread.BeginInvokeOnMainThread(() =>
                        {
                            StartConnection();
                        });

                connectedEvent?.WaitOne(TimeSpan.FromSeconds(connectionTimeout));

                return Authorized;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, new Dictionary<string, string>
                {
                     { "SpotifyClientId", configurationService.GetValue("SpotifyClientId") },
                     { "redirectUri", configurationService.GetValue("SpotifyClientRedirectUri") }
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

        public OAuthAuthenticator GetAuthenticator()
        {
            var id = configurationService.GetValue("SpotifyClientId");
            var secret = configurationService.GetValue("SpotifyClientSecret");
            var scopes = configurationService.GetValue("SpotifyClientScopes");
            var callback = configurationService.GetValue("SpotifyClientRedirectUri");
            Authenticator = new OAuthAuthenticator(id, secret, scopes, new Uri("https://accounts.spotify.com/authorize"), new Uri(callback), new Uri("https://accounts.spotify.com/api/token"), null, true);
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

        public override void DidDisconnectWithError(SPTAppRemote appRemote, NSError error)
        {
            try
            {
                Authorized = false;
                api = null;
                if (error.LocalizedRecoveryOptions != null)
                {
                    logger?.LogError(new Exception(error.LocalizedDescription));
                }

                Disconnected?.Invoke(this, new SpotifyDisconnectedEventArgs(false));
            }
            catch (ObjectDisposedException ex)
            {
                logger?.LogError(ex);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        public override void DidEstablishConnection(SPTAppRemote appRemote)
        {
            try
            {
                Authorized = api != null && api.Connected && !string.IsNullOrEmpty(Token);
                connectedEvent.Set();
            }
            catch (ObjectDisposedException ex)
            {
                logger?.LogError(ex);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        public override void DidFailConnectionAttemptWithError(SPTAppRemote appRemote, NSError error)
        {
            try
            {
                logger?.LogError(new Exception(error.Description), new Dictionary<string, string>
                {
                     { "SpotifyClientId", configurationService.GetValue("SpotifyClientId") },
                     { "redirectUri", configurationService.GetValue("SpotifyClientRedirectUri") }
                });

                connectedEvent.Set();
                tokenEvent.Set();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex);
                throw new Exception(error.LocalizedDescription);
            }
        }

        public void InstallSpotify()
        {
            var storeViewController = new SKStoreProductViewController();
            storeViewController.Delegate = this;
            storeViewController.LoadProduct(new StoreProductParameters((int)SPTAppRemote.SpotifyItunesItemIdentifier), (bool loaded, NSError error) =>
            {
                if ((error == null) && loaded)
                {
                    var window = UIApplication.SharedApplication.KeyWindow;
                    window?.RootViewController?.PresentModalViewController(storeViewController, true);
                }

                if (error != null)
                {
                    logger?.LogError(new NSErrorException(error));
                    throw new NSErrorException(error);
                }
            });
        }

        public bool IsSpotifyInstalled()
        {
            return UIApplication.SharedApplication.CanOpenUrl(new NSUrl(new NSString("spotify:")));
        }

        public void Disconnect()
        {
            try
            {
                if (api != null && api.Connected)
                {
                    api.Disconnect();
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
        }

        private void StartAuthorization(string songUri = "")
        {
            var isSpotifyInstalled = IsSpotifyInstalled();
            if (isSpotifyInstalled)
            {
                var clientId = configurationService.GetValue("SpotifyClientId");
                var redirectUri = configurationService.GetValue("SpotifyClientRedirectUri");
                var appConfig = new SPTConfiguration(clientId, NSUrl.FromString(redirectUri));
                api = new SPTAppRemote(appConfig, SPTAppRemoteLogLevel.Info);
                api.Delegate = this;
                api.AuthorizeAndPlayURI(songUri);
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
                api.ConnectionParameters.AccessToken = Token;
                api.Connect();
                return; 
            }

            connectedEvent.Set();
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
            catch (Exception ex)
            {
                logger?.LogError(ex);
            }

            return result;
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
