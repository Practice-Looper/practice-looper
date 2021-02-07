﻿using System;
using Emka.PracticeLooper.Mobile.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MappingsFactory = Emka3.PracticeLooper.Mappings;
using Emka.PracticeLooper.Mobile.Navigation;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using Device = Xamarin.Forms.Device;
using Xamarin.Essentials;
using Emka.PracticeLooper.Services.Contracts;
using System.Collections.Generic;
using Emka.PracticeLooper.Mobile.Themes;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Utils;
using System.Linq;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Emka.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Config.Contracts.Features;
using FilePicker = Emka.PracticeLooper.Mobile.Common.FilePicker;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Emka.PracticeLooper.Mobile
{
    [Preserve(AllMembers = true)]
    public partial class App : Application
    {
        public static string BannerAddUnitId { get; private set; }
        private MappingsFactory.Contracts.IResolver resolver;
        public App()
        {
            InitializeComponent();
        }

        protected async override void OnStart()
        {
            base.OnStart();

            try
            {
                VersionTracking.Track();
                InitApp();
                await InitNavigation().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                throw;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            DeviceDisplay.KeepScreenOn = !DeviceDisplay.KeepScreenOn;
        }

        protected override async void OnSleep()
        {
            DeviceDisplay.KeepScreenOn = !DeviceDisplay.KeepScreenOn;
            var audioPlayers = resolver.ResolveAll<IAudioPlayer>();
            var service = resolver.Resolve<ISpotifyApiService>();

            if (audioPlayers != null && audioPlayers.Any())
            {
                foreach (var player in audioPlayers)
                {
                    player.Pause(true);
                }
            }

            await service.PauseCurrentPlayback();
        }

        private void InitApp()
        {
            // Register common forms types

            resolver = MappingsFactory.Factory.GetResolver();
            resolver.RegisterSingleton(typeof(FilePicker), typeof(IFilePicker));
            resolver.RegisterSingleton(typeof(NavigationService), typeof(INavigationService));
            resolver.RegisterSingleton(typeof(SourcePicker), typeof(ISourcePicker));
            resolver.RegisterSingleton(typeof(FileAudioPlayer), typeof(IAudioPlayer));
            resolver.RegisterInstance(new DialogService(), typeof(IDialogService));
            resolver.Register(typeof(StringLocalizer), typeof(IStringLocalizer));
            // Build container after platform implementations have been registered
            resolver.BuildContainer();

            if (!DeviceDisplay.KeepScreenOn)
            {
                DeviceDisplay.KeepScreenOn = !DeviceDisplay.KeepScreenOn;
            }

            if (VersionTracking.IsFirstLaunchEver)
            {
                SecureStorage.RemoveAll();
            }
            var configService = resolver.Resolve<IConfigurationService>();
            var purchaseItems = configService.GetValue<Dictionary<string, object>>("PurchaseItems");
            var premiumItemId = Device.RuntimePlatform == Device.iOS ? purchaseItems["IosPremiumLifetime"].ToString() : purchaseItems["AndroidPremiumLifetime"].ToString();
            var premiumFeature = new PremiumFeature(premiumItemId);
            var featureRegistry = resolver.Resolve<IFeatureRegistry>();

            configService.SetValue("IsFirstLaunchEver", VersionTracking.IsFirstLaunchEver);
#if PREMIUM
            configService.SetSecureValue(premiumItemId, true);
#endif
            var hasPremium = configService.GetSecureValue<bool>(premiumItemId);
            featureRegistry.Add(premiumFeature, hasPremium);
        }

        private async Task InitNavigation()
        {
            ICollection<ResourceDictionary> mergedDictionaries = Current.Resources.MergedDictionaries;
            var configService = MappingsFactory.Factory.GetResolver().Resolve<IConfigurationService>() ?? throw new ArgumentNullException("configService");
            if (mergedDictionaries != null)
            {
                if (configService.GetValue(PreferenceKeys.ColorScheme, -1) == (int)AppTheme.Light)
                {
                    mergedDictionaries.Add(new LightTheme());
                }
                else if (configService.GetValue(PreferenceKeys.ColorScheme, -1) == (int)AppTheme.Dark)
                {
                    mergedDictionaries.Add(new DarkTheme());
                }
                else
                {
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        MainPage = new Page();
                    }

                    switch (AppInfo.RequestedTheme)
                    {
                        case AppTheme.Dark:
                            mergedDictionaries.Add(new DarkTheme());
                            configService.SetValue(PreferenceKeys.ColorScheme, (int)AppTheme.Dark);
                            break;
                        case AppTheme.Unspecified:
                        case AppTheme.Light:
                            mergedDictionaries.Add(new LightTheme());
                            configService.SetValue(PreferenceKeys.ColorScheme, (int)AppTheme.Light);
                            break;
                    }
                }
            }

            var navigationService = MappingsFactory.Factory.GetResolver()?.Resolve<INavigationService>();
            await navigationService?.InitializeAsync();
        }
    }
}
