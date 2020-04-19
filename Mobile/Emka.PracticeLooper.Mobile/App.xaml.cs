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
using Emka3.PracticeLooper.Config.Feature;
using Emka.PracticeLooper.Mobile.Views;
using Emka3.PracticeLooper.Services.Contracts.Common;
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
#if PREMIUM
        public static bool HasPremium = true;
#else
        public static bool HasPremium = false;
#endif

        public App()
        {
            InitializeComponent();
        }

        protected async override void OnStart()
        {
            base.OnStart();

            try
            {
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
            FeatureRegistry.Add<AdMobView>(!HasPremium);
            FeatureRegistry.Add<LoopButton>(HasPremium);
            FeatureRegistry.Add<IInterstitialAd>(!HasPremium);
            FeatureRegistry.Add<IPremiumAudioPlayer>(HasPremium && DeviceInfo.DeviceType == DeviceType.Physical, "Spotify");
            FeatureRegistry.Add<ISourcePicker>(HasPremium);

            // Register common forms types
            MappingsFactory.Contracts.IResolver resolver = MappingsFactory.Factory.GetResolver();
            resolver.RegisterSingleton(typeof(FilePicker), typeof(Common.IFilePicker));
            resolver.RegisterSingleton(typeof(NavigationService), typeof(INavigationService));
            resolver.RegisterSingleton(typeof(SourcePicker), typeof(ISourcePicker));
            resolver.Register(typeof(FileAudioPlayer), typeof(IAudioPlayer));
            resolver.RegisterInstance(new DialogService(), typeof(IDialogService));

            // Build container after platform implementations have been registered
            MappingsFactory.Factory.GetResolver().BuildContainer();

            if (!DeviceDisplay.KeepScreenOn)
            {
                DeviceDisplay.KeepScreenOn = !DeviceDisplay.KeepScreenOn;
            }
        }

        private void InitConfig()
        {
            ConfigurationService = Factory.GetConfigService();
            ConfigurationService.SetValue(nameof(HasPremium), HasPremium);
            // todo: unnötige attribute aussortieren und den einzelnen files!
            ConfigurationService.SetValue(nameof(Helpers.Secrets.SpotifyClientApiUri), Helpers.Secrets.SpotifyClientApiUri);
            ConfigurationService.SetValue(nameof(Helpers.Secrets.SpotifyApiLimit), Helpers.Secrets.SpotifyApiLimit);
            ConfigurationService.SetValue(nameof(Helpers.Secrets.SpotifyClientId), Helpers.Secrets.SpotifyClientId);
            ConfigurationService.SetValue(nameof(Helpers.Secrets.SpotifyClientRedirectUri), Helpers.Secrets.SpotifyClientRedirectUri);
            ConfigurationService.SetValue(nameof(Helpers.Secrets.SpotifyClientScopes), Helpers.Secrets.SpotifyClientScopes);
            ConfigurationService.SetValue(nameof(Helpers.Secrets.DbName), Helpers.Secrets.DbName);

            if (Device.RuntimePlatform == Device.iOS)
            {
                ConfigurationService.LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.Resources);
                BannerAddUnitId = Helpers.Secrets.AdmobIosTopBannerAdId;
                ConfigurationService.SetValue(nameof(Helpers.Secrets.AdmobIosInterstitialProjectAdId), Helpers.Secrets.AdmobIosInterstitialProjectAdId);
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                BannerAddUnitId = Helpers.Secrets.AdmobAndroidTopBanneAdId;
                ConfigurationService.LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                ConfigurationService.SetValue(nameof(Helpers.Secrets.SpotifyClientRequestCode), Helpers.Secrets.SpotifyClientRequestCode);
                ConfigurationService.SetValue(nameof(Helpers.Secrets.AdmobAndroidInterstitialProjectAdId), Helpers.Secrets.AdmobAndroidInterstitialProjectAdId);
            }
        }

        private async Task InitNavigation()
        {
            var navigationService = MappingsFactory.Factory.GetResolver()?.Resolve<INavigationService>();
            await navigationService?.InitializeAsync();

            ICollection<ResourceDictionary> mergedDictionaries = Current.Resources.MergedDictionaries;

            if (mergedDictionaries != null)
            {
                switch (AppInfo.RequestedTheme)
                {
                    case AppTheme.Unspecified:
                    case AppTheme.Dark:
                        mergedDictionaries.Add(new DarkTheme());
                        break;
                    case AppTheme.Light:
                        mergedDictionaries.Add(new LightTheme());
                        break;
                }
            }
        }
    }
}
