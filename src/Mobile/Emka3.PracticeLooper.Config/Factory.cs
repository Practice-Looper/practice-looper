// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
namespace Emka3.PracticeLooper.Config
{
    public static class Factory
    {
        public static IConfigurationService GetConfigService()
        {
            return ConfigurationService.Instance;
        }
    }
}
