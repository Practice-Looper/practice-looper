// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Config.Contracts
{
    /// <summary>
    /// App Configuration service.
    /// </summary>
    [Preserve(AllMembers = true)]
    public interface IConfigurationService
    {
        event EventHandler<string> ValueChanged;

        #region Methods
        /// <summary>
        /// Read configs from secrets file.
        /// </summary>
        void ReadConfigs();

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
        void SetValue(string key, object value, bool persist = default);

        /// <summary>
        /// Adds a value to config service async.
        /// </summary>
        /// <param name="key">key for value storage.</param>
        /// <param name="value">value to store.</param>
        Task SetValueAsync(string key, object value, bool persist = default);

        /// <summary>
        /// Gets the value for a generic type.
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="key">Key.</param>
        /// <typeparam name="T">The generic type parameter.</typeparam>
        T GetValue<T>(string key, T defaultValue = default);

        /// <summary>
        /// Gets the value for a generic type async.
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="key">Key.</param>
        /// <typeparam name="T">The generic type parameter.</typeparam>
        Task<T> GetValueAsync<T>(string key, T defaultValue = default);

        /// <summary>
        /// Gets the value for a specific async.
        /// </summary>
        /// <returns> Config value.</returns>
        /// <param name="key">Key.</param>
        Task<string> GetValueAsync(string key);
        #endregion Methods
    }
}
