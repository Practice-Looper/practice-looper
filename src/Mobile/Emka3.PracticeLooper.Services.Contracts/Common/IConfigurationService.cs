// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Threading.Tasks;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    /// <summary>
    /// App Configuration service.
    /// </summary>
    public interface IConfigurationService
    {

        #region Methods
        /// <summary>
        /// Initializes the configuration service.
        /// Use flat json strings only => "key" : "value"
        /// </summary>
        /// <param name="json">Json string representing the config file.</param>
        void Initialize(string json);

        /// <summary>
        /// Initializes the configuration service async.
        /// </summary>
        /// <param name="json">Json string representing the config file.</param>
        Task InitializeAsync(string json);

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
