// (c) Liveit. All rights reserved. 2017
// Maksim Kolesnik, David Symhoven

using System;
using System.Threading.Tasks;
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
        Task DeleteTokenAsync();
        Task<string> GetTokenAsync();
        Task UpdateTokenAsync(string token);
        #endregion Methods
    }
}
