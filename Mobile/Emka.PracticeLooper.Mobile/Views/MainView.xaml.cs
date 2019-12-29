using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    public partial class MainView : ContentPage
    {
        #region Ctor
        public MainView()
        {
            InitializeComponent();
            
            this.BindingContext = new MainViewModel();
        }
        #endregion

        #region Properties
        public string AdUnitId { get; private set; }
        #endregion

        #region Methods
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
        #endregion
    }
}
