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
using Xamarin.Essentials;
using Emka.PracticeLooper.Services.Contracts;
using System.Collections.Generic;
using Emka.PracticeLooper.Mobile.Themes;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Utils;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Emka.PracticeLooper.Mobile
{
    [Preserve(AllMembers = true)]
    public partial class App : Application
    {
        public IConfigurationService ConfigurationService { get; private set; }
        public static string BannerAddUnitId { get; private set; }

        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Factory.GetConfigService().GetValue("SyncFusionLicenseKey"));
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
            resolver.RegisterInstance(ConfigurationService, typeof(IConfigurationService));
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
            ConfigurationService.SetValue(PreferenceKeys.PremiumGeneral, true, true);
#endif
            var x = typeof(Preferences);
            ConfigurationService = new ConfigurationService(new PersistentConfigService());

            if (Device.RuntimePlatform == Device.iOS)
            {
                ConfigurationService.LocalPath = FileSystem.AppDataDirectory;
                BannerAddUnitId = ConfigurationService.GetValue("AdmobIosTopBannerAdId");
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                BannerAddUnitId = ConfigurationService.GetValue("AdmobAndroidTopBannerAdId");
                ConfigurationService.LocalPath = FileSystem.AppDataDirectory;
            }
        }

        private async Task InitNavigation()
        {
            ICollection<ResourceDictionary> mergedDictionaries = Current.Resources.MergedDictionaries;

            if (mergedDictionaries != null)
            {
                if (ConfigurationService.GetValue(PreferenceKeys.ColorScheme, -1) == (int)AppTheme.Light)
                {
                    mergedDictionaries.Add(new LightTheme());
                }
                else if (ConfigurationService.GetValue(PreferenceKeys.ColorScheme, -1) == (int)AppTheme.Dark)
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
