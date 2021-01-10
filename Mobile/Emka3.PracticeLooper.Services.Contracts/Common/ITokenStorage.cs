// (c) Liveit. All rights reserved. 2017
// Maksim Kolesnik, David Symhoven

using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.EventArgs;
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    /// <summary>
    /// Accounts manager for local storage of user accounts.
    /// </summary>
    [Preserve(AllMembers = true)]
    public interface ITokenStorage
    {
        #region Events
        event EventHandler<TokenChangedEventArgs> TokenChanged;
        #endregion Events

        #region Methods
        Task<bool> HasRefreshToken(AudioSourceType sourceType);
        Task DeleteTokenAsync();
        string GetAccessToken(AudioSourceType sourceType);
        Task<string> GetRefreshTokenAsync(AudioSourceType sourceType);
        void UpdateAccessToken(AudioSourceType sourceType, string token, int expiresIn);
        Task UpdateRefreshTokenAsync(AudioSourceType sourceType, string token);
        bool HasTokenExpired(AudioSourceType sourceType);
        #endregion Methods
    }
}
