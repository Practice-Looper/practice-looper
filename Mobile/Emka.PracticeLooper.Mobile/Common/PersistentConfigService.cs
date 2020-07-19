// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.Common
{
    public class PersistentConfigService : IPersistentConfigService
    {
        public T GetPersistedValue<T>(string key, T defaultValue)
        {
            Type t = typeof(Preferences);
            var result = t?
            .GetMethod("Get", new Type[] { typeof(string), typeof(T) })?
            .Invoke(null, new object[] { key, defaultValue });

            return (T)Convert.ChangeType(result, typeof(T));
        }

        public async Task<T> GetPersistedValueAsync<T>(string key, T defaultValue = default)
        {
            return await Task.Run(() => GetPersistedValue(key, defaultValue));
        }

        public void PersistValue<T>(string key, T value)
        {
            Type t = typeof(Preferences);
            t?
            .GetMethod("Set", new Type[] { typeof(string), typeof(T) })?
            .Invoke(null, new object[] { key, value });
        }

        public async Task PersistValueAsync<T>(string key, T value)
        {
            await Task.Run(() => PersistValue(key, value));
        }
    }
}
