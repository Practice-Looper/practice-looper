using System;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Mobile.Views;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MappingsFactory = Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using System.Collections.Generic;
using Emka.PracticeLooper.Mobile.Navigation;
using System.Threading.Tasks;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Emka.PracticeLooper.Mobile
{
    public partial class App : Application
    {
        public static IConfigurationService ConfigurationService { get; private set; }
        public static string BannerAddUnitId { get; private set; }

        public App()
        {
            InitializeComponent();
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        protected override async void OnStart()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.OnStart();
            // Handle when your app starts
            InitConfig();
            InitApp();
            await InitNavigation();
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
                // Register common forms types
                MappingsFactory.Contracts.IResolver resolver = MappingsFactory.Factory.GetResolver();
                resolver.Register(typeof(FilePicker), typeof(Common.IFilePicker));
                resolver.Register(typeof(NavigationService), typeof(INavigationService));
                resolver.Register(typeof(SourcePicker), typeof(ISourcePicker));
                resolver.Register(typeof(FileAudioPlayer), typeof(IAudioPlayer), AudioSourceType.Local.ToString());

                // Build container after platform implementations have been registered
                MappingsFactory.Factory.GetResolver().BuildContainer();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void InitConfig()
        {
            ConfigurationService = Factory.GetConfigService();
            if (Device.RuntimePlatform == Device.iOS)
            {
                ConfigurationService.LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.Resources);
                BannerAddUnitId = ConfigurationService.GetValue("admob:ios:TopBanner");
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                BannerAddUnitId = ConfigurationService.GetValue("admob:android:TopBanner");
                ConfigurationService.LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
        }

        private Task InitNavigation()
        {
            var navigationService = MappingsFactory.Factory.GetResolver().Resolve<INavigationService>();
            return navigationService.InitializeAsync();
        }
    }
}
