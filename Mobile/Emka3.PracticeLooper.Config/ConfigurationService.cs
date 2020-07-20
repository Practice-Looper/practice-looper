﻿// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;

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
        private IPersistentConfigService persistentConfigService;
        #endregion Fields

        #region Ctor
        public ConfigurationService(IPersistentConfigService persistentConfigService)
        {
            this.persistentConfigService = persistentConfigService;
            Initialize();
        }
        #endregion

        #region Events
        public event EventHandler<string> ValueChanged;
        #endregion

        #region Properties
        public string LocalPath { get; set; }

        public bool IsSpotifyInstalled { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize the config.
        /// </summary>
        private void Initialize()
        {
            configs = new Dictionary<string, object>();
        }

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
                return (T)configs[key];
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
                OnValueChanged(key);
            }

            if (persist)
            {
                persistentConfigService.PersistValue(key, value, value.GetType());
            }

            if (configs != null && !configs.ContainsKey(key) && !persist)
            {
                configs.Add(key, value);
            }
        }

        public async Task SetValueAsync(string key, object value, bool persist = default)
        {
            await Task.Run(() => SetValue(key, value)).ConfigureAwait(false);
        }
        #endregion Methods
    }
}
