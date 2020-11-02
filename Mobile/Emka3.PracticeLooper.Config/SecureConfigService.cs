// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;

namespace Emka3.PracticeLooper.Config
{
    [Preserve(AllMembers = true)]
    public class SecureConfigService : ISecureConfigService
    {
        public string GetSecureValue(string key)
        {
            var result = SecureStorage.GetAsync(key).Result;
            return result;
        }

        public async Task<string> GetSecureValueAsync(string key)
        {
            return await SecureStorage.GetAsync(key);
        }

        public void SetSecureValue(string key, object value)
        {
            SecureStorage.SetAsync(key, value.ToString()).Wait();
        }

        public async Task SetSecureValueAsync(string key, object value)
        {
            await SecureStorage.SetAsync(key, value.ToString());
        }
    }
}
