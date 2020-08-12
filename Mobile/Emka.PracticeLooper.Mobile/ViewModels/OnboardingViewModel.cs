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
        public int Position { get; set; }
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
                    ImageUrl = "WordLogo.png"
                },
                new OnboardingModel
                {
                    Title = "Onboarding",
                    Content = "Beschreibung",
                    ImageUrl = "WordLogo.png"
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
