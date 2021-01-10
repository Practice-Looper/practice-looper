// (c) Liveit. All rights reserved. 2017
// Maksim Kolesnik, David Symhoven

using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.EventArgs;
using Emka3.PracticeLooper.Utils;
namespace Emka3.PracticeLooper.Services.Common
{
    /// <summary>
    /// Saves and loads users login credentials or token.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class TokenStorage : ITokenStorage
    {
        #region Fields
        private const string SPOTIFY_REFRESH_TOKEN_KEY = "spotify_refresh_token";
        private string spotifyAccessToken;
        private DateTime spotifyTokenExpirationTime;
        private readonly ISecureRepository secureRepository;
        #endregion Ctor

        public TokenStorage(ISecureRepository secureRepository)
        {
            this.secureRepository = secureRepository;
            spotifyTokenExpirationTime = DateTime.Now;
        }

        #region Events
        public event EventHandler<TokenChangedEventArgs> TokenChanged;
        #endregion Events

        #region Methods

        public async Task DeleteTokenAsync()
        {
            await Task.Run(() => secureRepository.DeleteValueAsync(SPOTIFY_REFRESH_TOKEN_KEY));
        }

        public void UpdateAccessToken(AudioSourceType sourceType, string token, int expiresIn)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (sourceType == AudioSourceType.Spotify)
            {
                spotifyAccessToken = token;
                spotifyTokenExpirationTime = DateTime.Now.AddMinutes(expiresIn);
            }

            RaiseTokenChanged(new TokenChangedEventArgs(token));
        }

        void RaiseTokenChanged(TokenChangedEventArgs args) => TokenChanged?.Invoke(this, args);

        public string GetAccessToken(AudioSourceType sourceType)
        {
            if (sourceType == AudioSourceType.Spotify)
            {
                return spotifyAccessToken;
            }

            return string.Empty;
        }

        public async Task<string> GetRefreshTokenAsync(AudioSourceType sourceType)
        {
            if (sourceType == AudioSourceType.Spotify)
            {
                return await secureRepository.GetValueAsync(SPOTIFY_REFRESH_TOKEN_KEY);
            }

            return string.Empty;
        }

        public async Task<bool> HasRefreshToken(AudioSourceType sourceType)
        {
            string token = string.Empty;

            if (sourceType == AudioSourceType.Spotify)
            {
                token = await secureRepository.GetValueAsync(SPOTIFY_REFRESH_TOKEN_KEY);
            }

            return !string.IsNullOrWhiteSpace(token);
        }

        public async Task UpdateRefreshTokenAsync(AudioSourceType sourceType, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            await secureRepository.SetValueAsync(SPOTIFY_REFRESH_TOKEN_KEY, token);
        }

        public bool HasTokenExpired(AudioSourceType sourceType)
        {
            if (sourceType == AudioSourceType.Spotify)
            {
                return DateTime.Now > spotifyTokenExpirationTime;
            }

            return true;
        }
        #endregion Methods
    }
}
