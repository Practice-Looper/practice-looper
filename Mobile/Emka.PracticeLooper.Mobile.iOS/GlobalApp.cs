// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using Emka.PracticeLooper.Mobile.iOS.Common;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Plugin.InAppBilling;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.iOS
{
    internal static class GlobalApp
    {
        public static void Init()
        {
            ConfigurationService = Factory.GetConfigService();
            MappingsFactory.Contracts.IResolver resolver = MappingsFactory.Factory.GetResolver();
            resolver.RegisterSingleton(typeof(SpotifyLoader), typeof(ISpotifyLoader));
            resolver.Register(typeof(SpotifyAudioPlayer), typeof(IAudioPlayer));
            resolver.Register(typeof(InAppBillingVerifyPurchase), typeof(IInAppBillingVerifyPurchase));
            resolver.RegisterSingleton(typeof(AudioFileRepository), typeof(IFileRepository));
            resolver.RegisterSingleton(typeof(AudioMetadataReader), typeof(IAudioMetadataReader));
            resolver.RegisterSingleton(typeof(InterstitialAd), typeof(IInterstitialAd));
            resolver.RegisterSingleton(typeof(ConnectivityService), typeof(IConnectivityService));
        }

        internal static IConfigurationService ConfigurationService { get; private set; }
    }
}
