﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config;
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
        #endregion

        #region Ctor

        public SpotifyLoader()
        {
            this.configurationService = Factory.GetConfigService();
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
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    api.AuthorizeAndPlayURI(songUri);
                });

                try
                {
                    tokenEvent.WaitOne();
                    tokenEvent.Dispose();
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                    throw;
                }

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
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                    throw;
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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                tokenEvent = new AutoResetEvent(false);
                connectedEvent = new AutoResetEvent(false);
            });

            try
            {
                await Task.Run(() => Initialize(songUri));
                result = true;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                throw;
            }

            return result;
        }

        public override void DidDisconnectWithError(SPTAppRemote appRemote, NSError error)
        {
            Crashes.TrackError(new Exception(error.Description));
            connectedEvent.Reset();
        }

        public override void DidEstablishConnection(SPTAppRemote appRemote)
        {
            try
            {
                connectedEvent.Set();
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                throw;
            }

        }

        public override void DidFailConnectionAttemptWithError(SPTAppRemote appRemote, NSError error)
        {
            try
            {
                Crashes.TrackError(new Exception(error.Description));
                connectedEvent.Set();
                connectedEvent.Reset();
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                throw;
            }
        }
        #endregion
    }
}
