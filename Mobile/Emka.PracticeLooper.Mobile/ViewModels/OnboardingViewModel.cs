// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020

using System.Collections.ObjectModel;
using Emka3.PracticeLooper.Model.Common;
using Xamarin.Forms;
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
        private int position;
        private bool isSkipButtonVisible;
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
            SetSkipButtonVisible(true);

            Items = new ObservableCollection<OnboardingModel>
            {
                new OnboardingModel
                {
                    Title = AppResources.OnboardingView_Welcome_Title,
                    Content = AppResources.OnboardingView_Welcome_Description,
                    ImageUrl = "start.png"
                },
                new OnboardingModel
                {
                    Title = AppResources.OnboardingView_AddSong_Title,
                    ImageUrl = "addSong.png",
                    Icon = "\uf419"
                },
                new OnboardingModel
                {
                    Title = AppResources.OnboardingView_Slider_Title,
                    ImageUrl = "slider.png"
                },
                new OnboardingModel
                {
                    Title = AppResources.OnboardingView_Picker_Title,
                    ImageUrl = "picker.png"
                },
                new OnboardingModel
                {
                    Title = AppResources.OnboardingView_AddMarker_Title,
                    ImageUrl = "addMarker.png",
                    Icon = "\uf0c4"
                },
                new OnboardingModel
                {
                    Title = AppResources.OnboardingView_ShowMarker_Title,
                    ImageUrl = "showMarker.png",
                    Icon = "\U000f03a4"
                },
                new OnboardingModel
                {
                    Title = AppResources.OnboardingView_LetsStart_Title,
                    Content = AppResources.OnboardingView_LetsStart_Description,
                    ImageUrl = "letsStart.png",
                    StartButtonVisible = true
                }
            };
        }
        #endregion

        #region Methods
        private void SetSkipButtonVisible(bool isSkipButtonVisible)
            => IsSkipButtonVisible = isSkipButtonVisible;
        
        public bool IsSkipButtonVisible
        {
            get => isSkipButtonVisible;
            set
            {
                isSkipButtonVisible = value;
                NotifyPropertyChanged();
            }
        }

        public int Position
        {
            get => position;
            set
            {
                position = value;

           
                if (position == Items.Count - 1)
                {
                    SetSkipButtonVisible(false);
                } else
                {
                    SetSkipButtonVisible(true);
                }

                NotifyPropertyChanged();
            }
        }

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
