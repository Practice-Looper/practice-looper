
using System;
using System.Reflection;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.Messenger;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            foreach (var res in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                System.Diagnostics.Debug.WriteLine("found resource: " + res);
            }
        }

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
