// (c) Liveit. All rights reserved. 2017
// Maksim Kolesnik, David Symhoven

using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.EventArgs;
using Emka3.PracticeLooper.Utils;
namespace Emka3.PracticeLooper.Services.Common
{
    /// <summary>
    /// Saves and loads users login credentials or token.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class SpotifyTokenStorage : ITokenStorage
    {
        #region Fields
        private const string TOKEN_KEY = "spotify_token";
        private readonly ISecureRepository secureRepository;
        #endregion Ctor

        public SpotifyTokenStorage(ISecureRepository secureRepository)
        {
            this.secureRepository = secureRepository;
        }

        #region Events
        public event EventHandler<TokenChangedEventArgs> TokenChanged;
        #endregion Events

        #region Methods

        public async Task DeleteTokenAsync()
        {
            await Task.Run(() => secureRepository.DeleteValueAsync(TOKEN_KEY));
        }

        public async Task UpdateTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            await secureRepository.SetValueAsync(TOKEN_KEY, token);
            RaiseTokenChanged(new TokenChangedEventArgs(token));
        }

        void RaiseTokenChanged(TokenChangedEventArgs args) => TokenChanged?.Invoke(this, args);

        public async Task<string> GetTokenAsync()
        {
            return await secureRepository.GetValueAsync(TOKEN_KEY);
        }
        #endregion Methods
    }
}
