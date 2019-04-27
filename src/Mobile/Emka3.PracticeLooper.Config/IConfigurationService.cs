// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Threading.Tasks;

namespace Emka3.PracticeLooper.Config
{
    /// <summary>
    /// App Configuration service.
    /// </summary>
    public interface IConfigurationService
    {

        #region Properties
        /// <summary>
        /// Local path to store selected files.
        /// </summary>
        /// <value>The local path.</value>
        string LocalPath { get; set; }

        bool IsSpotifyInstalled { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the value for a specific key.
        /// </summary>
        /// <returns> Config value.</returns>
        /// <param name="key">Key.</param>
        string GetValue(string key);

        /// <summary>
        /// Gets the value for a generic type.
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="key">Key.</param>
        /// <typeparam name="T">The generic type parameter.</typeparam>
        T GetValue<T>(string key);

        /// <summary>
        /// Gets the value for a generic type async.
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="key">Key.</param>
        /// <typeparam name="T">The generic type parameter.</typeparam>
        Task<T> GetValueAsync<T>(string key);

        /// <summary>
        /// Gets the value for a specific async.
        /// </summary>
        /// <returns> Config value.</returns>
        /// <param name="key">Key.</param>
        Task<string> GetValueAsync(string key);
        #endregion Methods
    }
}
