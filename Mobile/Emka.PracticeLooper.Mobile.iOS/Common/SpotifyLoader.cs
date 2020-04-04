// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;
using Microsoft.AppCenter.Crashes;
using SpotifyBindings.iOS;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    [Preserve(AllMembers = true)]
    public class SpotifyLoader : SPTAppRemoteDelegate, ISpotifyLoader
    {
        #region Fields

        private SPTAppRemote api;
        static AutoResetEvent tokenEvent;
        static AutoResetEvent connectedEvent;
        private string token;
        private readonly IConfigurationService configurationService;
        private readonly ILogger logger;
        #endregion

        #region Ctor

        public SpotifyLoader(ILogger logger)
        {
            configurationService = Factory.GetConfigService();
            this.logger = logger;
        }
        #endregion

        #region Properties

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
                        tokenEvent.Set();
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex);
                        throw;
                    }
                }
            }
        }

        public bool Authorized => !string.IsNullOrEmpty(Token);
        #endregion

        #region Methods

        public void Initialize(string songUri = "")
        {
            var clientId = configurationService.GetValue("SpotifyClientId");
            var redirectUri = configurationService.GetValue("SpotifyClientRedirectUri");

            var appConfig = new SPTConfiguration(clientId, NSUrl.FromString(redirectUri));
            api = new SPTAppRemote(appConfig, SPTAppRemoteLogLevel.Info);

            if (GlobalApp.ConfigurationService.IsSpotifyInstalled)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    api.AuthorizeAndPlayURI(songUri);
                });

                tokenEvent.WaitOne();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    api.ConnectionParameters.AccessToken = Token;
                    api.Delegate = this;
                    api.Connect();
                });

                connectedEvent.WaitOne();
            }
            else
            {
                // prompt to install
            }
        }

        public async Task<bool> InitializeAsync(string songUri = "")
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                tokenEvent = new AutoResetEvent(false);
                connectedEvent = new AutoResetEvent(false);
            });

            await Task.Run(() => Initialize(songUri));
            return true;
        }

        public override void DidDisconnectWithError(SPTAppRemote appRemote, NSError error)
        {
            try
            {
                logger?.LogError(new Exception(error.Description));
                connectedEvent.Reset();
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
                logger?.LogError(new Exception(error.Description));
                connectedEvent.Set();
                connectedEvent.Reset();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex);
                throw;
            }
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
        #endregion
    }
}
