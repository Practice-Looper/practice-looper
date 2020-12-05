// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public partial class ProductPaywallView : ContentPage
    {
        public ProductPaywallView()
        {
            InitializeComponent();

            var resolver = Factory.GetResolver();
            BindingContext = new ProductPaywallViewModel(
                    resolver.Resolve<INavigationService>(),
                    resolver.Resolve<ILogger>(),
                    resolver.Resolve<IAppTracker>(),
                    resolver.Resolve<IInAppBillingService>(),
                    resolver.Resolve<IDialogService>(),
                    resolver.Resolve<IFeatureRegistry>());
        }
    }
}
