// Copyright (C) ${CopyrightHolder} - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Mappings.Contracts;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    public partial class OnboardingView : ContentPage
    {
        #region Fields
        private IResolver resolver;
        #endregion

        public OnboardingView()
        {
            InitializeComponent();
            resolver = Emka3.PracticeLooper.Mappings.Factory.GetResolver();

            BindingContext = new OnboardingViewModel(
                   resolver.Resolve<INavigationService>(),
                   resolver.Resolve<ILogger>(),
                   resolver.Resolve<IAppTracker>());
        }
    }
}
