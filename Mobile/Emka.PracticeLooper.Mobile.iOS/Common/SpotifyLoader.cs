// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;
using SpotifyBindings.iOS;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class SpotifyLoader : SPTAppRemoteDelegate, ISpotifyLoader
    {
        #region Fields

        private SPTAppRemote api;
        static AutoResetEvent tokenEvent;
        static AutoResetEvent connectedEvent;
        private string token;
        private readonly IConfigurationService configurationService;
        #endregion

        #region Ctor

        public SpotifyLoader()
        {
            this.configurationService = Factory.GetConfigService();

            tokenEvent = new AutoResetEvent(false);
            connectedEvent = new AutoResetEvent(false);
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
                    tokenEvent.Set();
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
                //SPTAppRemote.CheckIfSpotifyAppIsActive((isActive) =>
                //{
                //    if (!isActive)
                //    {
                //        // prompt user to authorize sloopy
                //        api.AuthorizeAndPlayURI(songUri);
                //    }
                //});

                api.AuthorizeAndPlayURI(songUri);

                try
                {
                    tokenEvent.WaitOne();
                    tokenEvent.Dispose();
                }
                catch (System.Exception ex)
                {
                    // todo: log
                }

                //api.ConnectionParameters.AccessToken = Token;
                //api.Delegate = this;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    api.ConnectionParameters.AccessToken = Token;
                    api.Delegate = this;
                    api.Connect();
                });

                try
                {
                    connectedEvent.WaitOne();
                    connectedEvent.Dispose();
                }
                catch (System.Exception ex)
                {
                    // todo: log
                }
            }
            else
            {
                // prompt to install
            }
        }

        public async Task<bool> InitializeAsync(string songUri = "")
        {
            bool result = false;

            try
            {
                await Task.Run(() => Initialize(songUri));
                result = true;
            }
            catch (System.Exception ex)
            {
                // todo: log
                throw;
            }

            return result;
        }

        public override void DidDisconnectWithError(SPTAppRemote appRemote, NSError error)
        {
            Initialize();
        }

        public override void DidEstablishConnection(SPTAppRemote appRemote)
        {
            try
            {
                connectedEvent.Set();
            }
            catch (System.Exception ex)
            {
                // todo: log
            }

        }

        public override void DidFailConnectionAttemptWithError(SPTAppRemote appRemote, NSError error)
        {

        }
        #endregion
    }
}
