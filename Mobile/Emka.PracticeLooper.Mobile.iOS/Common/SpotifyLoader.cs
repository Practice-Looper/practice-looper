﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emka.PracticeLooper.Services.Contracts;
using Emka.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model;
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

        private SPTAppRemote api;
        static AutoResetEvent tokenEvent;
        static AutoResetEvent connectedEvent;
        private string token;
        private readonly IConfigurationService configurationService;
        private readonly ILogger logger;
        private readonly IDialogService dialogService;
        private readonly IStringLocalizer stringLocalizer;
        #endregion

        #region Ctor

        public SpotifyLoader(ILogger logger,
            IDialogService dialogService,
            IStringLocalizer stringLocalizer,
            IConfigurationService configurationService)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.stringLocalizer = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
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

        public event EventHandler Disconnected;
        #endregion

        #region Methods

        public bool Initialize(string songUri = "")
        {
            try
            {
                var connectionTimeout = configurationService.GetValue<int>("SpotifyConnectionTimeOut");
                if (!configurationService.GetValue<bool>(PreferenceKeys.IsSpotifyInstalled))
                {
                    var installSpotify = dialogService.ShowConfirmAsync(
                            stringLocalizer.GetLocalizedString("Hint_Caption_SpotifyMissing"),
                            stringLocalizer.GetLocalizedString("Hint_Content_SpotifyMissing"),
                            stringLocalizer.GetLocalizedString("Cancel"),
                            stringLocalizer.GetLocalizedString("Ok")).Result;

                    if (installSpotify)
                    {
                        InstallSpotify();
                    }

                    return false;
                }

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

                Authorized = api != null && api.Connected && !string.IsNullOrEmpty(Token);
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
            await configurationService.SetValueAsync(PreferenceKeys.IsSpotifyInstalled, IsSpotifyInstalled());
            return await Task.Run(() => Initialize(songUri));
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

            var clientId = configurationService.GetValue("SpotifyClientId");
            var redirectUri = configurationService.GetValue("SpotifyClientRedirectUri");
            var appConfig = new SPTConfiguration(clientId, NSUrl.FromString(redirectUri));
            api = new SPTAppRemote(appConfig, SPTAppRemoteLogLevel.Info);
            api.Delegate = this;
            api.AuthorizeAndPlayURI(songUri);
        }

        private void StartConnection()
        {
            api.ConnectionParameters.AccessToken = Token;
            api.Connect();
        }

        private void InstallSpotify()
        {
            var storeViewController = new SKStoreProductViewController();
            storeViewController.Delegate = this;
            //storeViewController.Finished += OnStoreViewControllerFinished;
            storeViewController.LoadProduct(new StoreProductParameters((int)SPTAppRemote.SpotifyItunesItemIdentifier), (bool loaded, NSError error) =>
            {
                if ((error == null) && loaded)
                {
                    var window = UIApplication.SharedApplication.KeyWindow;
                    window?.RootViewController?.PresentModalViewController(storeViewController, true);
                    tokenEvent.Set();
                    connectedEvent.Set();
                }
                if (error != null)
                {
                    logger?.LogError(new NSErrorException(error));
                    // todo: show dialog
                    throw new NSErrorException(error);
                }
            });
        }

        private bool IsSpotifyInstalled()
        {
            return UIApplication.SharedApplication.CanOpenUrl(new NSUrl(new NSString("spotify:")));
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
        #endregion
    }
}
