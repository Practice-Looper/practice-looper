// (c) Liveit. All rights reserved. 2017
// Maksim Kolesnik, David Symhoven

using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.EventArgs;
using Xamarin.Essentials;

namespace Emka3.PracticeLooper.Services.Common
{
    /// <summary>
    /// Saves and loads users login credentials or token.
    /// </summary>
    public class AccountManager : IAccountManager
    {
        #region Fields
        private const string TOKEN_KEY = "spotify_token";
        #endregion Ctor

        #region Events
        public event EventHandler<TokenChangedEventArgs> TokenChanged;
        #endregion Events

        #region Methods

        public async Task DeleteTokenAsync()
        {
            try
            {
                await Task.Run(() => SecureStorage.RemoveAll());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            try
            {
                await SecureStorage.SetAsync(TOKEN_KEY, token);
                RaiseTokenChanged(new TokenChangedEventArgs(token));
            }
            catch (Exception)
            {
                throw;
            }
        }

        void RaiseTokenChanged(TokenChangedEventArgs args) => TokenChanged?.Invoke(this, args);

        public async Task<string> GetTokenAsync()
        {
            try
            {
                return await SecureStorage.GetAsync(TOKEN_KEY);
            }
            catch (Exception)
            {
                // Possible that device doesn't support secure storage on device.
                throw;
            }
        }
        #endregion Methods
    }
}
