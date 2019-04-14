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
            InitAdMob();
            var navPage = new NavigationPage(new MainPage());
            navPage.BarBackgroundColor = ColorConstants.Background;
            navPage.BarTextColor = ColorConstants.Secondary;
            MainPage = navPage;

            InitApp();

            var filePicker = MappingsFactory.Factory.GetResolver().Resolve<Common.IFilePicker>();
            var sourcePicker = MappingsFactory.Factory.GetResolver().Resolve<ISourcePicker>();
            var audioPlayer = MappingsFactory.Factory.GetResolver().Resolve<IAudioPlayer>();
            var dbRepository = MappingsFactory.Factory.GetResolver().Resolve<IRepository<Session>>();
            dbRepository.Init();
            var bindingContext = new MainViewModel(filePicker, audioPlayer, sourcePicker, dbRepository);

            MainPage.BindingContext = bindingContext;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
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
                resolver.RegisterInstance(new SourcePicker(MainPage), typeof(ISourcePicker));

                // Build container after platform implementations have been registered
                MappingsFactory.Factory.GetResolver().BuildContainer();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void InitAdMob()
        {
            ConfigurationService = Factory.GetConfigService();

            if (Device.RuntimePlatform == Device.iOS)
                BannerAddUnitId = ConfigurationService.GetValue("admob:ios:TopBanner");
            else if (Device.RuntimePlatform == Device.Android)
                BannerAddUnitId = ConfigurationService.GetValue("admob:android:TopBanner");
        }
    }
}
