// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using Emka.PracticeLooper.Mobile.iOS.Common;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;
using SpotifyBindings.iOS;
using UIKit;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.iOS
{
    internal static class GlobalApp
    {
        private static bool HasPremium = false;
        public static void Init()
        {
            ConfigurationService = Factory.GetConfigService();
            MappingsFactory.Contracts.IResolver resolver = MappingsFactory.Factory.GetResolver();

            ConfigurationService.IsSpotifyInstalled = UIApplication.SharedApplication.CanOpenUrl(new NSUrl(new NSString("spotify:")));
            if (ConfigurationService.IsSpotifyInstalled)
            {
                resolver.RegisterSingleton(typeof(SpotifyAudioPlayer), typeof(IAudioPlayer));
                resolver.RegisterSingleton(typeof(SpotifyLoader), typeof(ISpotifyLoader));
            }         

            resolver.Register(typeof(AudioFileRepository), typeof(IFileRepository));
            resolver.RegisterSingleton(typeof(AudioMetadataReader), typeof(IAudioMetadataReader));
        }

        internal static IConfigurationService ConfigurationService { get; private set; }
        internal static SPTAppRemote SPTRemoteApi { get; private set; }
    }
}
