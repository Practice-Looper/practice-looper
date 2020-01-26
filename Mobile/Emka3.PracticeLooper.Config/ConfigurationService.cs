// Copyright (C) Emka3 - All Rights Reserved
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

namespace Emka3.PracticeLooper.Config
{
    /// <summary>
    /// Configuration service imlementation.
    /// </summary>
    internal class ConfigurationService : IConfigurationService
    {
        #region Fields
        /// <summary>
        /// Config strings.
        /// </summary>
        IDictionary<string, object> configs;

        private static ConfigurationService instance;
        #endregion Fields

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Emka3.PracticeLooper.Services.ConfigurationService"/> class.
        /// </summary>
        private ConfigurationService()
        {
            Initialize();
        }
        #endregion

        #region Properties
        public static ConfigurationService Instance
        {
            get
            {
                return instance ?? (instance = new ConfigurationService());
            }
        }

        public string LocalPath { get; set; }

        public bool IsSpotifyInstalled { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize the config.
        /// </summary>
        private void Initialize()
        {
            //// Load config
            //string json;
            //using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Emka3.PracticeLooper.Config.App.config.json"))
            //using (var reader = new StreamReader(stream))
            //{
            //    json = reader.ReadToEnd();
            //}

            //if (string.IsNullOrEmpty(json))
            //{
            //    throw new ArgumentNullException(nameof(json));
            //}

            //try
            //{
            //    // remove comment properties, since we don't need them in code.
            //    JObject conf = JObject.Parse(json);
            //    conf.Property("comment").Remove();

            //    // crate dictionary.
            //    configs = JsonConvert.DeserializeObject<Dictionary<string, object>>(conf.ToString());
            //    conf = null;
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        private async Task InitializeAsync()
        {
            await Task.Run(() => Initialize());
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

        public T GetValue<T>(string key)
        {
            if (configs != null && !string.IsNullOrEmpty(key) && configs.ContainsKey(key))
            {
                return (T)configs[key];
            }

            return default;
        }

        public async Task<T> GetValueAsync<T>(string key)
        {
            return await Task.Run(() => GetValue<T>(key));
        }

        public void SetValue(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                // todo : log
                throw new ArgumentException(nameof(key));
            }

            if (configs != null && !configs.ContainsKey(key))
            {
                configs.Add(key, value);
            }
        }

        public async Task SetValueAsync(string key, object value)
        {
            await Task.Run(() => SetValue(key, value)).ConfigureAwait(false);
        }
        #endregion Methods
    }
}
