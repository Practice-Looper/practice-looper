using System.Reflection;
using Emka.PracticeLooper.Mobile.ViewModels;
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

        void OnDraggingCompleted(object sender, System.EventArgs e)
        {
            var viewModel = BindingContext as MainViewModel;
            if(viewModel != null)
            {
                viewModel.UpdateMinMaxValues();
            }
        }
    }
}
