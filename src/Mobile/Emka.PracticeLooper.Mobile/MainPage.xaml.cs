
using System;
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
        private MainViewModel context;
        public MainPage()
        {
            InitializeComponent();

            //MessagingCenter.Subscribe<MainViewModel>(this, MessengerKeys.GetAudioSource, async (obj) =>
            //{
            //    var action = await DisplayActionSheet("Select Source", "Cancel", null, "File", "Spotify");
            //    if (action != "Cancel" && action.Equals("File"))
            //    {
            //        //filePicker.ShowPicker();

            //        try
            //        {
            //            FileData fileData = await CrossFilePicker.Current.PickFile();
            //            if (fileData == null)
            //                return; // user canceled file picking

            //            string fileName = fileData.FileName;
            //            string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);

            //            System.Console.WriteLine("File name chosen: " + fileName);
            //            System.Console.WriteLine("File data: " + contents);
            //        }
            //        catch (Exception ex)
            //        {
            //            System.Console.WriteLine("Exception choosing file: " + ex.ToString());
            //        }
            //    }
            //});

        }

        protected override void OnAppearing()
        {

            //var filePicker = Factory.GetResolver().Resolve<Common.IFilePicker>();
            //var sourcePicker = Factory.GetResolver().Resolve<ISourcePicker>();
            //var audioPlayer = Factory.GetResolver().Resolve<IAudioPlayer>();
            //this.context = new MainViewModel(filePicker, audioPlayer, sourcePicker);
            //this.BindingContext = this.context;
        }
    }
}
