// (c) Liveit. All rights reserved. 2017
// Maksim Kolesnik, David Symhoven

using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.EventArgs;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    /// <summary>
    /// Accounts manager for local storage of user accounts.
    /// </summary>
    public interface IAccountManager
    {
        #region Events
        /// <summary>
        /// Occurs when the jwt token changed.
        /// </summary>
        event EventHandler<TokenChangedEventArgs> TokenChanged;
        #endregion Events

        #region Methods
        ///// <summary>
        ///// Saves new account async.
        ///// </summary>
        ///// <param name="creds">Creds.</param>
        //Task SaveAccountAsync(Account creds);

        ///// <summary>
        ///// Gets a singleaccount async.
        ///// </summary>
        ///// <returns>The account.</returns>
        ///// <param name="includePassword">If set to <c>true</c> include password.</param>
        //Task<Account> GetAccountAsync(bool includePassword = false);

        ///// <summary>
        ///// Gets a single account.
        ///// </summary>
        ///// <returns>The account.</returns>
        ///// <param name="includePassword">If set to <c>true</c> include password.</param>
        //Account GetAccount(bool includePassword = false);

        ///// <summary>
        ///// Deletes all accounts async.
        ///// </summary>
        //Task DeleteAllAccountsAsync();

        ///// <summary>
        ///// Deletes an single account async.
        ///// </summary>
        ///// <returns>The account async.</returns>
        //Task DeleteAccountAsync(Account creds);

        ///// <summary>
        ///// Gets all accounts async.
        ///// </summary>
        ///// <returns>All accounts async.</returns>
        //Task<IList<Account>> GetAllAccountsAsync();

        /// <summary>
        /// Updates the jwt token.
        /// </summary>
        /// <param name="token">Token.</param>

        Task DeleteTokenAsync();
        Task<string> GetTokenAsync();
        Task UpdateTokenAsync(string token);
        #endregion Methods
    }
}
