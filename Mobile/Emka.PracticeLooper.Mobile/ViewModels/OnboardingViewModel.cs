// Copyright (C) ${CopyrightHolder} - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020

using System.Collections.ObjectModel;
using System.Windows.Input;
using Emka3.PracticeLooper.Model.Common;
using Xamarin.Forms;
using Emka.PracticeLooper.Mobile.Views;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class OnboardingViewModel : ViewModelBase
    {
        
        #region Fields
        private Command skipCommand;
        #endregion

        #region Properties
        public ObservableCollection<OnboardingModel> Items { get; set; }
        public Command SkipCommand => skipCommand ?? (skipCommand = new Command(async (o) => await StartMainPageAsync(o)));
        #endregion

        #region Ctor
        public OnboardingViewModel(
            INavigationService navigationService,
            ILogger logger,
            IAppTracker appTracker)
            : base(navigationService, logger, appTracker)
        {

            Items = new ObservableCollection<OnboardingModel>
            {
                new OnboardingModel
                {
                    Title = "Willkommen bei Practice Looper",
                    Content = "Practice, Loop - Loop, Practice",
                    ImageUrl = "OnboardingIntro.png"
                },
                new OnboardingModel
                {
                    Title = "Füge einen neuen Song hinzu",
                    Content = "Klicke auf \uf419 \uf641 um entweder einen neuen Song aus Spotify oder aus deiner lokalen Musikbibliothek hinzuzufügen.",
                    ImageUrl = "OnboardingAddSong.png"
                },
                new OnboardingModel
                {
                    Title = "Wähle eine untere Grenze",
                    Content = "Wähle die untere Grenze entweder mit dem Slider oder klicke auf die untere Grenze um den Picker zu öffnen.",
                    ImageUrl = "OnboardingStartPicker.png"
                },
                new OnboardingModel
                {
                    Title = "Wähle eine obere Grenze",
                    Content = "Mit dem Picker fällt es leichter Zeiträume genauer einzustellen.",
                    ImageUrl = "OnboardingEndPicker.png"
                },
                new OnboardingModel
                {
                    Title = "Füge eine Markierung hinzu",
                    Content = "Klicke auf \uf0c4 um eine neue Markierung zu setzen.",
                    ImageUrl = "OnboardingAddMarker.png"
                },
                new OnboardingModel
                {
                    Title = "Bearbeite deine Markierungen",
                    Content = "Klicke auf eine Markierung um diese abzuspielen oder wische nach links um diese zu löschen.",
                    ImageUrl = "OnboardingShowMarker.png"
                },
                new OnboardingModel
                {
                    Title = "Lets Loop!",
                    Content = "Füge jetzt deinen ersten Song hinzu. \n Viel Spaß!",
                    ImageUrl = "OnboardingLetsStart.png",
                    ButtonVisible = true
                }
            };
        }
        #endregion

        #region Methods
        private async Task StartMainPageAsync(object obj)
        {
            await NavigationService.NavigateToAsync<MainViewModel>();
        }

        public override Task InitializeAsync(object parameter)
        {
            return Task.CompletedTask;
        }
       
        #endregion
    }
}
