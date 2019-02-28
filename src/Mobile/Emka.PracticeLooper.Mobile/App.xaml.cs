using System;
using System.IO;
using System.Reflection;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Common;
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
            var config = Factory.GetResolver().Resolve<IConfigurationService>();
            InitApp();
            MainPage = new MainPage();
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
