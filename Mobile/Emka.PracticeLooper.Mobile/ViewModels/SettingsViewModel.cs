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
        private Command purchaseItemCommand;
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
            Products = new ObservableCollection<InAppBillingProductViewModel>();
            HasProducts = true;
        }
        #endregion

        #region Properties
        public Command PurchaseItemCommand => purchaseItemCommand ?? new Command(async o => await ExecutePurchaseItemCommand(o));
        public string AppVersion => VersionTracking.CurrentVersion;
        public ObservableCollection<InAppBillingProductViewModel> Products { get; set; }
        public bool IsBusy
        {
            get => isBusy; set
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
            HasProducts = true;
            if (inAppBillingService.Products == null || !inAppBillingService.Products.Any())
            {
                await inAppBillingService.FetchOfferingsAsync();
            }

            var tmpProducts = inAppBillingService.Products.Select(p => new InAppBillingProductViewModel(p.Value)).ToList();

            Products.Clear();
            foreach (var item in tmpProducts)
            {
                Products.Add(item);
            }

            HasProducts = Products.Any();
            IsBusy = false;
        }

      

        private async Task ExecutePurchaseItemCommand(object o)
        {
            if (o is InAppBillingProductViewModel product)
            {
                if (product.Purchased)
                {
                    await dialogService.ShowAlertAsync(AppResources.Hint_Content_AlreadyPurchased, AppResources.Hint_Caption_AlreadyPurchased);
                    return;
                }

                IsBusy = true;
                try
                {
                    var (success, error, cancelledByUser) = await inAppBillingService.PurchaseProductAsync(product.Model.Package);

                    if (!success && !cancelledByUser)
                    {
                        await dialogService.ShowAlertAsync(error ?? AppResources.Error_Content_General, AppResources.Error_Caption);
                    }

                    if (success && !cancelledByUser)
                    {
                        //unlock premium
                    }                    
                }
                catch (Exception ex)
                {
                    await Logger.LogErrorAsync(ex);
                    await dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
        #endregion
    }
}
