using System.Linq;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Config;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public partial class MainView : ContentPage
    {
        private readonly IConfigurationService configService;

        #region Ctor
        public MainView()
        {
            InitializeComponent();
            configService = Factory.GetConfigService();
            configService.ValueChanged += ConfigService_ValueChanged;
            BindingContext = new MainViewModel();
        }

        private void ConfigService_ValueChanged(object sender, string e)
        {
            if (e == PreferenceKeys.PremiumGeneral)
            {
                ToggleAd();
            }
        }
        #endregion

        #region Properties
        public string AdUnitId { get; private set; }
        #endregion

        #region Methods
        protected override void OnAppearing()
        {
            SettingsImage.Color = (Color)Application.Current.Resources["PrimaryColor"];
            ToggleAd();
        }

        private void ToggleAd()
        {
            if (configService.GetValue<bool>(PreferenceKeys.PremiumGeneral))
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
