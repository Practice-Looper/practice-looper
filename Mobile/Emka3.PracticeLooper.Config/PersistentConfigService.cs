// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Xamarin.Essentials;

namespace Emka3.PracticeLooper.Config
{
    public class PersistentConfigService : IPersistentConfigService
    {
        #region Fields

        private readonly Type preferencesType;
        #endregion

        #region Ctor

        public PersistentConfigService()
        {
            preferencesType = typeof(Preferences) ?? throw new NullReferenceException(nameof(preferencesType));
        }
        #endregion

        #region Methods

        public T GetPersistedValue<T>(string key, T defaultValue)
        {
            var result = preferencesType
            .GetMethod("Get", new Type[] { typeof(string), typeof(T) })
            .Invoke(null, new object[] { key, defaultValue });

            return (T)Convert.ChangeType(result, typeof(T));
        }

        public async Task<T> GetPersistedValueAsync<T>(string key, T defaultValue = default)
        {
            return await Task.Run(() => GetPersistedValue(key, defaultValue));
        }

        public void PersistValue(string key, object value, Type type)
        {
            try
            {
                preferencesType
                    .GetMethod("Set", new Type[] { typeof(string), type })
                    .Invoke(null, new object[] { key, value });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task PersistValueAsync(string key, object value, Type type)
        {
            await Task.Run(() => PersistValue(key, value, type));
        }

        public void ClearValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            Preferences.Clear(key);
        }

        public async Task ClearValueAsync(string key)
        {
            await Task.Run(() => { ClearValue(key); });
        }
        #endregion
    }
}
