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

        public string AdUnitId { get; private set; }

        protected override void OnAppearing()
        {
            AdUnitId = App.BannerAddUnitId;

        }

        void OnDraggingCompleted(object sender, System.EventArgs e)
        {
            var viewModel = BindingContext as MainViewModel;
            if (viewModel != null)
            {
                viewModel.UpdateMinMaxValues();
            }
        }

        void OnDeleteSession(object sender, System.EventArgs e)
        {
            var menuItem = ((MenuItem)sender);
            var mainViewModel = BindingContext as MainViewModel;
            if (mainViewModel != null)
            {
                mainViewModel.DeleteSessionCommand.Execute(menuItem.CommandParameter);
            }
        }
    }
}
