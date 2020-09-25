// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Collections.Generic;
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Config.Contracts
{
    [Preserve(AllMembers = true)]
    public interface IConfigurationLoader
    {
        IDictionary<string, object> LoadConfiguration(string path);
    }
}
