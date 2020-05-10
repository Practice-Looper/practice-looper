using System.Linq;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
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

            var resolver = Emka3.PracticeLooper.Mappings.Factory.GetResolver();
            BindingContext = new MainViewModel(resolver.Resolve<IInterstitialAd>(),
                resolver.Resolve<IRepository<Session>>(),
                resolver.Resolve<IRepository<Loop>>(),
                resolver.Resolve<IDialogService>(),
                resolver.Resolve<IFileRepository>(),
                resolver.Resolve<ISourcePicker>(),
                resolver.Resolve<ISpotifyLoader>(),
                configService,
                resolver.Resolve<Common.IFilePicker>());
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
