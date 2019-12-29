// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.iOS.Common;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Model.Player;
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
        public static TaskCompletionSource<bool> SpotifyTokenCompletionSource;
        public static void Init()
        {
            SpotifyTokenCompletionSource = new TaskCompletionSource<bool>();
            MappingsFactory.Contracts.IResolver resolver = MappingsFactory.Factory.GetResolver();
            resolver.Register(typeof(FileAudioPlayer), typeof(IAudioPlayer), AudioSourceType.Local.ToString());
            resolver.RegisterSingleton(typeof(SpotifyAudioPlayer), typeof(IAudioPlayer), AudioSourceType.Spotify.ToString());
            resolver.Register(typeof(AudioFileRepository), typeof(IFileRepository));
            resolver.RegisterSingleton(typeof(SpotifyLoader), typeof(ISpotifyLoader));
            ConfigurationService = Factory.GetConfigService();

            //var clientId = ConfigurationService.GetValue("auth:spotify:client:id");
            //var redirectUri = ConfigurationService.GetValue("auth:spotify:client:uri:redirect");

            //var appConfig = new SPTConfiguration(clientId, NSUrl.FromString(redirectUri));
            //SPTRemoteApi = new SPTAppRemote(appConfig, SPTAppRemoteLogLevel.Error);
            ConfigurationService.IsSpotifyInstalled = UIApplication.SharedApplication.CanOpenUrl(new NSUrl(new NSString("spotify:")));
        }

        internal static IConfigurationService ConfigurationService { get; private set; }
        internal static SPTAppRemote SPTRemoteApi { get; private set; }
    }
}
