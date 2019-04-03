using System.Reflection;
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
    }
}
