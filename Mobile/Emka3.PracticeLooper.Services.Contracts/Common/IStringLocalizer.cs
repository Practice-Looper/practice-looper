// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka3.PracticeLooper.Utils;

namespace Emka.PracticeLooper.Mobile.Common
{
    [Preserve(AllMembers = true)]
    public interface IStringLocalizer
    {
        string GetLocalizedString(string key);
        Task<string> GetLocalizedStringAsync(string key);
    }
}
