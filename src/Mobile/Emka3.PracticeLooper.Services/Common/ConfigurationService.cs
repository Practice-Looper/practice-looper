// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Emka3.PracticeLooper.Services.Common
{
    /// <summary>
    /// Configuration service imlementation.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        #region Fields
        /// <summary>
        /// Config strings.
        /// </summary>
        IDictionary<string, object> configs;
        #endregion Fields

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Emka3.PracticeLooper.Services.ConfigurationService"/> class.
        /// </summary>
        public ConfigurationService()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize the config.
        /// </summary>
        public void Initialize(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json));
            }

            try
            {
                // remove comment properties, since we don't need them in code.
                JObject conf = JObject.Parse(json);
                conf.Property("comment").Remove();

                // crate dictionary.
                configs = JsonConvert.DeserializeObject<Dictionary<string, object>>(conf.ToString());
                conf = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task InitializeAsync(string json)
        {
            await Task.Run(() => Initialize(json));
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

            return default(T);
        }

        public async Task<T> GetValueAsync<T>(string key)
        {
            return await Task.Run(() => GetValue<T>(key));
        }
        #endregion Methods
    }
}
