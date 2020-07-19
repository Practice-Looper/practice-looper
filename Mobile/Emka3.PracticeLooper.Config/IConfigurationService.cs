// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading.Tasks;

namespace Emka3.PracticeLooper.Config
{
    /// <summary>
    /// App Configuration service.
    /// </summary>
    public interface IConfigurationService
    {
        event EventHandler<string> ValueChanged;

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
        /// Adds a value to config service
        /// </summary>
        /// <param name="key">key for value storage.</param>
        /// <param name="value">value to store.</param>
        void SetValue(string key, object value);

        /// <summary>
        /// Adds a value to config service async.
        /// </summary>
        /// <param name="key">key for value storage.</param>
        /// <param name="value">value to store.</param>
        Task SetValueAsync(string key, object value);

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

        /// <summary>
        /// Saves a value permanently.
        /// </summary>
        /// <typeparam name="T">Generic type of value</typeparam>
        /// <param name="key">key to fetch stored value afterwards</param>
        /// <param name="value">value to store</param>
        void PersistValue<T>(string key, T value);

        /// <summary>
        /// Get the specified persisted value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns>Either found value or predefined default</returns>
        T GetPersistedValue<T>(string key, T defaultValue = default);
        #endregion Methods
    }
}
