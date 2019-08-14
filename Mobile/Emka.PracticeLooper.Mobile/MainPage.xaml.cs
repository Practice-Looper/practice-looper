using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Mobile.Views;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile
{
    public partial class MainPage : ContentPage
    {
        #region Fields
        private readonly Common.IFilePicker filePicker;
        #endregion

        #region Ctor
        public MainPage(Common.IFilePicker filePicker)
        {
            InitializeComponent();
            this.filePicker = filePicker;
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

        async void OnPickSource(object sender, System.EventArgs e)
        {
            var picker = new SourcePicker(this);
            var source = await picker.SelectFileSource();

            switch (source)
            {
                case "File":
                    var newFile = await filePicker.ShowPicker();
                    // file is null when user cancelled file picker!
                    if (newFile != null)
                    {
                        var mainViewModel = BindingContext as MainViewModel;
                        if (mainViewModel != null)
                        {
                            mainViewModel.CreateSessionCommand.Execute(newFile);
                        }
                    }

                    break;
                case "Spotify":
                    var spotifyApiService = Factory.GetResolver().Resolve<ISpotifyApiService>();
                    var spotifyLoader = Factory.GetResolver().Resolve<ISpotifyLoader>();
                    var sessionsRepository = Factory.GetResolver().Resolve<IRepository<Session>>();
                    await Navigation.PushAsync(new SpotifySearchView { BindingContext = await SpotifySearchViewModel.CreateAsync(spotifyApiService, spotifyLoader, sessionsRepository) });
                    break;
                default:
                    break;
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
