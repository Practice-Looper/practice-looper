// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config;
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
        static AutoResetEvent installedEvent;
        private string token;
        private readonly IConfigurationService configurationService;
        private readonly ILogger logger;
        private readonly IDialogService dialogService;
        #endregion

        #region Ctor

        public SpotifyLoader(ILogger logger, IDialogService dialogService)
        {
            configurationService = Factory.GetConfigService();
            this.logger = logger;
            this.dialogService = dialogService;
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

        #region Methods

        public bool Initialize(string songUri = "")
        {
            try
            {
                if (!configurationService.IsSpotifyInstalled)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        InstallSpotify();
                    });

                    installedEvent.WaitOne();
                    return false;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StartAuthorization(songUri);
                });

                tokenEvent?.WaitOne();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StartConnection();
                });

                connectedEvent?.WaitOne();

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
            installedEvent = new AutoResetEvent(false);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                configurationService.IsSpotifyInstalled = IsSpotifyInstalled();
            });

            return await Task.Run(() => Initialize(songUri));
        }

        public async override void DidDisconnectWithError(SPTAppRemote appRemote, NSError error)
        {
            try
            {
                Authorized = false;
                api = null;
                if (error.LocalizedRecoveryOptions != null)
                {
                    logger?.LogError(new Exception(error.LocalizedDescription));
                    await dialogService.ShowAlertAsync("Oops, lost connection to Spotify!");
                }
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

        public async override void DidFailConnectionAttemptWithError(SPTAppRemote appRemote, NSError error)
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
                await dialogService.ShowAlertAsync("Oops, failed to connecto to Spotify.");
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
            // todo: show dialog and ask user if go to appstore
            var storeViewController = new SKStoreProductViewController();
            storeViewController.Delegate = this;
            //storeViewController.Finished += OnStoreViewControllerFinished;
            storeViewController.LoadProduct(new StoreProductParameters((int)SPTAppRemote.SpotifyItunesItemIdentifier), (bool loaded, NSError error) =>
            {
                if ((error == null) && loaded)
                {
                    var window = UIApplication.SharedApplication.KeyWindow;
                    window?.RootViewController?.PresentModalViewController(storeViewController, true);
                    installedEvent.Set();
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
        #endregion
    }
}
