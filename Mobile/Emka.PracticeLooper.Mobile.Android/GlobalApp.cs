// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using Android.App;
using Android.Content.PM;
using Emka.PracticeLooper.Mobile.Droid.Common;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Microsoft.AppCenter.Crashes;
using Xamarin.Essentials;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.Droid
{
    internal static class GlobalApp
    {
        public static void Init()
        {
            // todo: move spotify namespace to secrets
            ConfigurationService = Factory.GetConfigService();
            ConfigurationService.IsSpotifyInstalled = IsAppInstalled("com.spotify.music");
            MappingsFactory.Contracts.IResolver resolver = MappingsFactory.Factory.GetResolver();

            resolver.RegisterSingleton(typeof(InterstitialAd), typeof(IInterstitialAd));
            resolver.Register(typeof(AudioFileRepository), typeof(IFileRepository));
            resolver.RegisterSingleton(typeof(AudioMetadataReader), typeof(IAudioMetadataReader));

            if (ConfigurationService.IsSpotifyInstalled)
            {
                resolver.RegisterSingleton(typeof(SpotifyAudioPlayer), typeof(IAudioPlayer));
                resolver.RegisterSingleton(typeof(SpotifyLoader), typeof(ISpotifyLoader));
            }
        }

        internal static IConfigurationService ConfigurationService { get; private set; }
        internal static bool HasPermissionToWriteExternalStorage { get; set; }
        internal static Activity MainActivity { get; set; }
        public static bool IsAppInstalled(string packageName)
        {
            try
            {
                PackageManager pm = Application.Context.PackageManager;
                pm.GetPackageInfo(packageName, PackageInfoFlags.Activities);
                return true;
            }
            catch (PackageManager.NameNotFoundException ex)
            {
                return false;
            }
        }
    }
}
