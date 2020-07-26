// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using Android.App;
using Emka.PracticeLooper.Mobile.Droid.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Plugin.InAppBilling;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.Droid
{
    internal static class GlobalApp
    {
        public static void Init()
        {
            MappingsFactory.Contracts.IResolver resolver = MappingsFactory.Factory.GetResolver();
            resolver.RegisterSingleton(typeof(InterstitialAd), typeof(IInterstitialAd));
            resolver.RegisterSingleton(typeof(AudioFileRepository), typeof(IFileRepository));
            resolver.RegisterSingleton(typeof(AudioMetadataReader), typeof(IAudioMetadataReader));
            resolver.Register(typeof(SpotifyAudioPlayer), typeof(IAudioPlayer));
            resolver.Register(typeof(InAppBillingVerifyPurchase), typeof(IInAppBillingVerifyPurchase));
            resolver.RegisterSingleton(typeof(SpotifyLoader), typeof(ISpotifyLoader));
            resolver.RegisterSingleton(typeof(ConnectivityService), typeof(IConnectivityService));
        }

        internal static bool HasPermissionToWriteExternalStorage { get; set; }
        internal static Activity MainActivity { get; set; }
    }
}
