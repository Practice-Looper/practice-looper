using System;
using Emka.PracticeLooper.Mobile.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MappingsFactory = Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Config;
using Emka.PracticeLooper.Mobile.Navigation;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using Device = Xamarin.Forms.Device;
using static Emka3.PracticeLooper.Config.Secrets;
using Xamarin.Essentials;
using Emka.PracticeLooper.Services.Contracts;
using System.Collections.Generic;
using Emka.PracticeLooper.Mobile.Themes;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Emka.PracticeLooper.Mobile
{
    public partial class App : Application
    {
        public static IConfigurationService ConfigurationService { get; private set; }
        public static string BannerAddUnitId { get; private set; }

        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(GetSecrets().SyncFusionLicenseKey);
            InitializeComponent();
        }

        protected async override void OnStart()
        {
            base.OnStart();

            try
            {
                VersionTracking.Track();
                InitConfig();
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

        protected override void OnSleep()
        {
            DeviceDisplay.KeepScreenOn = !DeviceDisplay.KeepScreenOn;
        }

        private void InitApp()
        {
            // Register common forms types
            MappingsFactory.Contracts.IResolver resolver = MappingsFactory.Factory.GetResolver();
            resolver.RegisterSingleton(typeof(FilePicker), typeof(IFilePicker));
            resolver.RegisterSingleton(typeof(NavigationService), typeof(INavigationService));
            resolver.RegisterSingleton(typeof(SourcePicker), typeof(ISourcePicker));
            resolver.RegisterSingleton(typeof(FileAudioPlayer), typeof(IAudioPlayer));
            resolver.RegisterInstance(new DialogService(), typeof(IDialogService));
            resolver.Register(typeof(StringLocalizer), typeof(IStringLocalizer));
            // Build container after platform implementations have been registered
            MappingsFactory.Factory.GetResolver().BuildContainer();

            if (!DeviceDisplay.KeepScreenOn)
            {
                DeviceDisplay.KeepScreenOn = !DeviceDisplay.KeepScreenOn;
            }
        }

        private void InitConfig()
        {
#if PREMIUM
            Preferences.Set(PreferenceKeys.PremiumGeneral, true);
#endif
            ConfigurationService = Factory.GetConfigService();
            ConfigurationService.SetValue(PreferenceKeys.PremiumGeneral, Preferences.Get(PreferenceKeys.PremiumGeneral, default(bool)));
            // todo: unnötige attribute aussortieren und den einzelnen files!
            ConfigurationService.SetValue("SpotifyClientApiUri", GetSecrets().SpotifyClientApiUri);
            ConfigurationService.SetValue("SpotifyApiLimit", GetSecrets().SpotifyApiLimit);
            ConfigurationService.SetValue("SpotifyClientId", GetSecrets().SpotifyClientId);
            ConfigurationService.SetValue("SpotifyClientRedirectUri", GetSecrets().SpotifyClientRedirectUri);
            ConfigurationService.SetValue("SpotifyClientScopes", GetSecrets().SpotifyClientScopes);
            ConfigurationService.SetValue("DbName", GetSecrets().DbName);

            if (Device.RuntimePlatform == Device.iOS)
            {
                ConfigurationService.LocalPath = FileSystem.AppDataDirectory;
                BannerAddUnitId = GetSecrets().AdmobIosTopBannerAdId;
                ConfigurationService.SetValue("AdmobIosInterstitialProjectAdId", GetSecrets().AdmobIosInterstitialProjectAdId);
                ConfigurationService.SetValue("InAppIosPremiumGeneral", GetSecrets().InAppIosPremiumGeneral);
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                BannerAddUnitId = GetSecrets().AdmobAndroidTopBanneAdId;
                ConfigurationService.LocalPath = FileSystem.AppDataDirectory;

                ConfigurationService.SetValue("SpotifyClientRequestCode", GetSecrets().SpotifyClientRequestCode);
                ConfigurationService.SetValue("AdmobAndroidInterstitialProjectAdId", GetSecrets().AdmobAndroidInterstitialProjectAdId);
                ConfigurationService.SetValue("InAppAndroidPremiumGeneral", GetSecrets().InAppAndroidPremiumGeneral);
            }
        }

        private async Task InitNavigation()
        {
            ICollection<ResourceDictionary> mergedDictionaries = Current.Resources.MergedDictionaries;

            if (mergedDictionaries != null)
            {
                if (Preferences.Get(PreferenceKeys.ColorScheme, -1) == (int)AppTheme.Light)
                {
                    mergedDictionaries.Add(new LightTheme());
                }
                else if (Preferences.Get(PreferenceKeys.ColorScheme, -1) == (int)AppTheme.Dark)
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
                            break;
                        case AppTheme.Unspecified:
                        case AppTheme.Light:
                            mergedDictionaries.Add(new LightTheme());
                            break;
                    }
                }
            }

            var navigationService = MappingsFactory.Factory.GetResolver()?.Resolve<INavigationService>();
            await navigationService?.InitializeAsync();
        }
    }
}
