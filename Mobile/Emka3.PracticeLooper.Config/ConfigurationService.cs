// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Config
{
    /// <summary>
    /// Configuration service imlementation.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class ConfigurationService : IConfigurationService
    {
        #region Fields
        /// <summary>
        /// Config strings.
        /// </summary>
        IDictionary<string, object> configs;
        private readonly IPersistentConfigService persistentConfigService;
        private readonly IConfigurationLoader configurationLoader;
        private readonly ISecureConfigService secureConfigService;
        #endregion Fields

        #region Ctor
        public ConfigurationService(
            IPersistentConfigService persistentConfigService,
            IConfigurationLoader configurationLoader,
            ISecureConfigService secureConfigService)
        {
            configs = new Dictionary<string, object>();
            this.persistentConfigService = persistentConfigService ?? throw new ArgumentNullException(nameof(persistentConfigService));
            this.configurationLoader = configurationLoader ?? throw new ArgumentNullException(nameof(configurationLoader));
            this.secureConfigService = secureConfigService ?? throw new ArgumentNullException(nameof(secureConfigService));
        }
        #endregion

        #region Events
        public event EventHandler<string> ValueChanged;
        #endregion

        #region Properties
        public bool IsSpotifyInstalled { get; set; }
        #endregion

        #region Methods
        private void OnValueChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                ValueChanged?.Invoke(this, value);
            }
        }

        public string GetValue(string key)
        {
            if (configs != null && !string.IsNullOrEmpty(key) && configs.ContainsKey(key))
            {
                return (string)configs[key];
            }

            return key;
        }

        public async Task<string> GetValueAsync(string key)
        {
            return await Task.Run(() => GetValue(key));
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (configs != null && !string.IsNullOrEmpty(key) && configs.ContainsKey(key))
            {
                var value = (T)Convert.ChangeType(configs[key], typeof(T));
                return value;
            }

            var persistedValue = persistentConfigService.GetPersistedValue(key, defaultValue);
            return persistedValue;
        }

        public async Task<T> GetValueAsync<T>(string key, T defaultValue = default)
        {
            return await Task.Run(() => GetValue<T>(key, defaultValue));
        }

        public void SetValue(string key, object value, bool persist = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(nameof(key));
            }

            if (configs != null && configs.ContainsKey(key))
            {
                configs[key] = value;
            }

            if (persist)
            {
                persistentConfigService.PersistValue(key, value, value.GetType());
            }

            if (configs != null && !configs.ContainsKey(key) && !persist)
            {
                configs.Add(key, value);
            }

            OnValueChanged(key);
        }

        public async Task SetValueAsync(string key, object value, bool persist = default)
        {
            await Task.Run(() => SetValue(key, value, persist)).ConfigureAwait(false);
        }

        public void ReadConfigs(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var loadedSecrets = configurationLoader.LoadConfiguration(path) ?? throw new Exception("could not read configs!");
            foreach (var loadedKeyValuePair in loadedSecrets)
            {
                configs.Add(loadedKeyValuePair);
            }
        }

        public void SetSecureValue(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null || string.IsNullOrWhiteSpace(value?.ToString()))
            {
                throw new ArgumentNullException(nameof(value));
            }

            secureConfigService.SetSecureValue(key, value.ToString());
            OnValueChanged(key);
        }

        public async Task SetSecureValueAsync(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null || string.IsNullOrWhiteSpace(value?.ToString()))
            {
                throw new ArgumentNullException(nameof(value));
            }

            await secureConfigService.SetSecureValueAsync(key, value.ToString());
            OnValueChanged(key);
        }

        public string GetSecureValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var result = secureConfigService.GetSecureValue(key);
            return result;
        }

        public T GetSecureValue<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(key);
            }

            var value = GetSecureValue(key);
            var result = value == string.Empty || value == null ? default : (T)Convert.ChangeType(value, typeof(T));
            return result;
        }

        public async Task<string> GetSecureValueAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(key);
            }

            return await secureConfigService.GetSecureValueAsync(key);
        }

        public async Task<T> GetSecureValueAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(key);
            }

            var value = await GetSecureValueAsync(key);
            var result = value == string.Empty || value == null ? default : (T)Convert.ChangeType(value, typeof(T));
            return result;
        }

        public void ClearValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (configs.ContainsKey(key))
            {
                configs.Remove(key);
            }

            persistentConfigService.ClearValue(key);
            secureConfigService.ClearValue(key);
            OnValueChanged(key);
        }

        public async Task ClearValueAsync(string key)
        {
            await Task.Run(() => { ClearValue(key); });
        }
        #endregion Methods
    }
}
