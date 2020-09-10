﻿// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        #endregion Fields

        #region Ctor
        public ConfigurationService(IPersistentConfigService persistentConfigService, IConfigurationLoader configurationLoader)
        {
            configs = new Dictionary<string, object>();
            this.persistentConfigService = persistentConfigService ?? throw new ArgumentNullException(nameof(persistentConfigService));
            this.configurationLoader = configurationLoader ?? throw new ArgumentNullException(nameof(configurationLoader));
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
                return (T)Convert.ChangeType(configs[key], typeof(T));
            }

            return persistentConfigService.GetPersistedValue(key, defaultValue);
        }

        public async Task<T> GetValueAsync<T>(string key, T defaultValue = default)
        {
            return await Task.Run(() => GetValue<T>(key));
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
        #endregion Methods
    }
}
