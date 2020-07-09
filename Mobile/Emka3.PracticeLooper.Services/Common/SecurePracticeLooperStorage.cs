// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;

namespace Emka3.PracticeLooper.Services.Common
{
    [Preserve(AllMembers = true)]
    public class SecurePracticeLooperStorage : ISecureRepository
    {
        public SecurePracticeLooperStorage()
        {
        }

        public async Task DeleteAllValuesAsync()
        {
            await Task.Run(() => SecureStorage.RemoveAll());
        }

        public async Task DeleteValueAsync(string key)
        {
            await Task.Run(() => SecureStorage.Remove(key));
        }

        public async Task<string> GetValueAsync(string key)
        {
            return await SecureStorage.GetAsync(key);
        }

        public async Task SetValueAsync(string key, string value)
        {
            await SecureStorage.SetAsync(key, value);
        }
    }
}
