// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Emka.PracticeLooper.Mobile.iOS.Common;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Contracts.Player;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.iOS
{
    internal static class GlobalApp
    {
        public static void Init()
        {
            MappingsFactory.Contracts.IResolver resolver = MappingsFactory.Factory.GetResolver();
            resolver.Register(typeof(FileAudioPlayer), typeof(IAudioPlayer));
            ConfigurationService = Factory.GetConfigService();
        }

        internal static IConfigurationService ConfigurationService { get; private set; }
    }
}
