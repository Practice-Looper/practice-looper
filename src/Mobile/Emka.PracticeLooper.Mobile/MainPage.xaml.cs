
using Emka.PracticeLooper.Mobile.Messenger;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Forms;
using Xamarin.RangeSlider.Forms;

namespace Emka.PracticeLooper.Mobile
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel context;
        public MainPage()
        {
            InitializeComponent();
            var filePicker = Factory.GetResolver().Resolve<IFilePicker>();
            var audioPlayer = Factory.GetResolver().Resolve<IAudioPlayer>();
            this.context = new MainViewModel(filePicker, audioPlayer);
            this.BindingContext = this.context;

            MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.GetAudioSource, async (obj) =>
            {
                var action = await DisplayActionSheet("Select Source", "Cancel", null, "File", "Spotify");
                if (action != "Cancel" && action.Equals("File"))
                {
                    this.context.SelectedAudioSource = action;
                    filePicker.ShowPicker();
                }
            });
        }
    }
}
