// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Config.Contracts
{
    [Preserve(AllMembers = true)]
    public interface ISecureConfigService
    {
        string GetSecureValue(string key);
        Task<string> GetSecureValueAsync(string key);
        void SetSecureValue(string key, object value);
        Task SetSecureValueAsync(string key, object value);
        void ClearValue(string key);
        Task ClearValueAsync(string key);
    }
}
