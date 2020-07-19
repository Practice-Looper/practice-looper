// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Config.Contracts
{
    [Preserve(AllMembers = true)]
    public interface IPersistentConfigService
    {
        /// <summary>
        /// Saves a value permanently.
        /// </summary>
        /// <typeparam name="T">Generic type of value</typeparam>
        /// <param name="key">key to fetch stored value afterwards</param>
        /// <param name="value">value to store</param>
        void PersistValue<T>(string key, T value);

        /// <summary>
        /// Saves a value permanently async.
        /// </summary>
        /// <typeparam name="T">Generic type of value</typeparam>
        /// <param name="key">key to fetch stored value afterwards</param>
        /// <param name="value">value to store</param>
        Task PersistValueAsync<T>(string key, T value);

        /// <summary>
        /// Get the specified persisted value async.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns>Either found value or predefined default</returns>
        Task<T> GetPersistedValueAsync<T>(string key, T defaultValue = default);

        /// Get the specified persisted value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns>Either found value or predefined default</returns>
        T GetPersistedValue<T>(string key, T defaultValue = default);
    }
}
