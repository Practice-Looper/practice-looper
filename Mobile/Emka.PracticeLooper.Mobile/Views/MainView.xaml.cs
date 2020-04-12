using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Config.Feature;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public partial class MainView : ContentPage
    {
        #region Ctor
        public MainView()
        {
            InitializeComponent();
            BindingContext = new MainViewModel();
        }
        #endregion

        #region Properties
        public string AdUnitId { get; private set; }
        #endregion

        #region Methods
        protected override void OnAppearing()
        {
            if (!FeatureRegistry.IsEnabled<AdMobView>())
            {
                MainGrid.Children.Remove(AdmobBanner);
            }
            else
            {
                AdUnitId = App.BannerAddUnitId;
            }
        }

        void OnDraggingCompleted(object sender, System.EventArgs e)
        {
            var viewModel = BindingContext as MainViewModel;
            if (viewModel != null && viewModel.CurrentLoop != null)
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
