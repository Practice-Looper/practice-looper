// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Xamarin.Auth;

namespace Emka3.PracticeLooper.Services.Rest
{
    /// <summary>
    /// Spotify authentication service.
    /// </summary>
    public class SpotifyAuthenticationService : IAuthenticationService
    {
        #region Fields
        private CancellationTokenSource cts;
        private readonly IAuthenticator<OAuth2Authenticator> authenticator;
        //IAppConfigService configService;
        #endregion Fields

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Emka3.PracticeLooper.Services.SpotifyAuthenticationService"/> class.
        /// </summary>
        public SpotifyAuthenticationService(IAuthenticator<OAuth2Authenticator> authenticator)
        {
            this.authenticator = authenticator;
        }
        #endregion

        public async Task<AuthenticationResponse> AuthenticateAsync()
        {
            var authCompletionSource = new TaskCompletionSource<EventArgs>();
            AuthenticationResponse response = null;

            void authenticatorCompleted(object o, AuthenticatorCompletedEventArgs e)
            {
                try
                {
                    if (e.IsAuthenticated)
                    {
                        response = new AuthenticationResponse
                        {
                            //Authenticated = e.IsAuthenticated,
                            //Token = e.Account.Properties["access_token"]
                        };
                    }

                    authCompletionSource.TrySetResult(e);
                }
                catch (Exception)
                {
                    // todo: log error
                    authCompletionSource.TrySetResult(new AuthenticatorCompletedEventArgs(null));
                }
            }

            void authenticatorFailed(object o, AuthenticatorErrorEventArgs e)
            {
                try
                {
                    response = new AuthenticationResponse
                    {
                        //Authenticated = false,
                        //Token = string.Empty
                    };
                    authCompletionSource.TrySetResult(e);
                }
                catch (Exception)
                {
                    // todo: log error
                    authCompletionSource.TrySetResult(new AuthenticatorCompletedEventArgs(null));
                }
            }

            try
            {
                authenticator.Instance.Completed += authenticatorCompleted;
                authenticator.Instance.Error += authenticatorFailed;
                await authCompletionSource.Task;

                return response;
            }
            catch (Exception)
            {
                // todo handle and log the exception
                return null;
            }
            finally
            {
                authenticator.Instance.Completed -= authenticatorCompleted;
                authenticator.Instance.Error -= authenticatorFailed;
            }
        }
    }
}
