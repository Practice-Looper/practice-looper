using System;
using System.IO;
using System.Reflection;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Mobile.Views;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Emka.PracticeLooper.Mobile
{
    public partial class App : Application
    {
        public static IConfigurationService ConfigurationService { get; private set; }

        public App()
        {
            InitializeComponent();
            var navPage = new NavigationPage(new MainPage());
            navPage.BarBackgroundColor = ColorConstants.Background;
            navPage.BarTextColor = ColorConstants.Secondary;
            MainPage = navPage;

            InitApp();

            var filePicker = Factory.GetResolver().Resolve<Common.IFilePicker>();
            var sourcePicker = Factory.GetResolver().Resolve<ISourcePicker>();
            var audioPlayer = Factory.GetResolver().Resolve<IAudioPlayer>();
            var dbRepository = Factory.GetResolver().Resolve<IRepository<Session>>();
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
                Emka3.PracticeLooper.Mappings.Contracts.IResolver resolver = Factory.GetResolver();
                resolver.Register(typeof(FilePicker), typeof(Common.IFilePicker));
                resolver.RegisterInstance(new SourcePicker(MainPage), typeof(ISourcePicker));

                // Build container after platform implementations have been registered
                Factory.GetResolver().BuildContainer();

                // Load config
                string jsonConfig;
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Emka.PracticeLooper.Mobile.App.config.json"))
                using (var reader = new StreamReader(stream))
                {
                    jsonConfig = reader.ReadToEnd();
                }

                ConfigurationService = Factory.GetResolver().Resolve<IConfigurationService>();
                ConfigurationService.Initialize(jsonConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
