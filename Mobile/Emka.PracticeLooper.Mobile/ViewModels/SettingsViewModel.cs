// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SettingsViewModel : ViewModelBase
    {
        #region Fields
        private readonly IConfigurationService configService;
        private readonly IDialogService dialogService;
        private readonly IInAppBillingService inAppBillingService;
        private Command showProductPaywallCommand;
        private bool isBusy;
        private bool hasProducts;
        #endregion

        #region Ctor

        public SettingsViewModel(IConfigurationService configService, ILogger logger, IDialogService dialogService, IAppTracker appTracker, INavigationService navigationService, IInAppBillingService inAppBillingService)
            : base(navigationService, logger, appTracker)
        {
            this.configService = configService ?? throw new ArgumentNullException(nameof(configService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.inAppBillingService = inAppBillingService ?? throw new ArgumentNullException(nameof(inAppBillingService));
            Products = new ObservableCollection<InAppPurchaseProductViewModel>();
            HasProducts = true;
        }
        #endregion

        #region Properties
        public Command ShowProductPaywallCommand => showProductPaywallCommand ?? new Command(async (o) => await ExecuteShowProductPaywallCommand(o));
        public string AppVersion => VersionTracking.CurrentVersion;
        public ObservableCollection<InAppPurchaseProductViewModel> Products { get; set; }
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasProducts
        {
            get => hasProducts;
            set
            {
                hasProducts = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Methods

        public async override Task InitializeAsync(object parameter)
        {
            IsBusy = true;

            if (!inAppBillingService.Initialized)
            {
                inAppBillingService.Init();
            }

            var (couldFetchOfferings, fetchOfferingsErrorReason) = await inAppBillingService.FetchOfferingsAsync();

            if (!couldFetchOfferings)
            {
                await dialogService.ShowAlertAsync(AppResources.Error_Caption, fetchOfferingsErrorReason);
                return;
            }

            var (couldRestorePurchases, restorePurchasesErrorReason) = await inAppBillingService.RestorePurchasesAsync();

            if (!couldRestorePurchases)
            {
                await dialogService.ShowAlertAsync(AppResources.Error_Caption, restorePurchasesErrorReason);
            }

            var tmpProducts = inAppBillingService.Products.Select(p => p.Value).Select(p => new InAppPurchaseProductViewModel(p));

            Products.Clear();
            foreach (var item in tmpProducts)
            {
                Products.Add(item);
            }

            HasProducts = Products.Any();
            IsBusy = false;
        }

        private async Task ExecuteShowProductPaywallCommand(object parameter)
        {
            if (parameter is InAppPurchaseProductViewModel product)
            {
                await NavigationService.NavigateToAsync<ProductPaywallViewModel>(product);
            }
        }
        #endregion
    }
}
