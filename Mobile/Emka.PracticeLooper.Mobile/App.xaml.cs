using System;
using Emka.PracticeLooper.Mobile.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MappingsFactory = Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Config;
using Emka.PracticeLooper.Mobile.Navigation;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Device = Xamarin.Forms.Device;
using Emka3.PracticeLooper.Config.Feature;
using Emka.PracticeLooper.Mobile.Views;
using Emka3.PracticeLooper.Services.Contracts.Common;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Emka.PracticeLooper.Mobile
{
    public partial class App : Application
    {
        public static IConfigurationService ConfigurationService { get; private set; }
        public static string BannerAddUnitId { get; private set; }
        public static bool HasPremium = true;

        public App()
        {
            InitializeComponent();
            //MainPage = new CustomNavigationView(new MainView());
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        protected async override void OnStart()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.OnStart();
            AppCenter.Start($"ios={Helpers.Secrets.AppCenterIos};android={Helpers.Secrets.AppCenterAndroid}", typeof(Analytics), typeof(Crashes));
            // Handle when your app starts

            InitConfig();
            InitApp();
            await InitNavigation().ConfigureAwait(true);
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        private void InitApp()
        {
            try
            {
                FeatureRegistry.Add<AdMobView>(!HasPremium);
                FeatureRegistry.Add<IInterstitialAd>(!HasPremium);
                FeatureRegistry.Add<IPremiumAudioPlayer>(HasPremium, "Spotify");
                FeatureRegistry.Add<ISourcePicker>(HasPremium);

                // Register common forms types
                MappingsFactory.Contracts.IResolver resolver = MappingsFactory.Factory.GetResolver();
                resolver.Register(typeof(FilePicker), typeof(Common.IFilePicker));
                resolver.Register(typeof(NavigationService), typeof(INavigationService));
                resolver.Register(typeof(SourcePicker), typeof(ISourcePicker));
                resolver.Register(typeof(FileAudioPlayer), typeof(IAudioPlayer));

                // Build container after platform implementations have been registered
                MappingsFactory.Factory.GetResolver().BuildContainer();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Crashes.TrackError(ex);
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
            Analytics.TrackEvent(Helpers.Secrets.SpotifyClientId);
            Analytics.TrackEvent(Helpers.Secrets.DbName);

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
        }
    }
}
