// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Config.Contracts.Features;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class ProductPaywallViewModel : ViewModelBase
    {
        #region Fields
        private bool isBusy;
        private readonly IInAppBillingService inAppBillingService;
        private readonly IDialogService dialogService;
        private readonly IFeatureRegistry featureRegistry;
        private Command purchaseItemCommand;
        private InAppPurchaseProductViewModel product;
        #endregion

        #region Ctor

        public ProductPaywallViewModel(
            INavigationService navigationService,
            ILogger logger,
            IAppTracker appTracker,
            IInAppBillingService inAppBillingService,
            IDialogService dialogService,
            IFeatureRegistry featureRegistry)
            : base(navigationService, logger, appTracker)
        {
            this.inAppBillingService = inAppBillingService ?? throw new ArgumentNullException(nameof(inAppBillingService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.featureRegistry = featureRegistry ?? throw new ArgumentNullException(nameof(featureRegistry));
        }
        #endregion

        #region Properties

        public Command PurchaseItemCommand => purchaseItemCommand ?? new Command(async () => await ExecutePurchaseItemCommand());

        public InAppPurchaseProductViewModel Product
        {
            get => product;
            private set
            {
                product = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                isBusy = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Methods

        public override Task InitializeAsync(object parameter)
        {
            IsBusy = true;
            if (parameter is InAppPurchaseProductViewModel product)
            {
                switch (product.ProductId)
                {
                    case var _ when product.ProductId == featureRegistry.GetFeature<PremiumFeature>()?.StoreId:
                        product.Image = "premium.png";
                        break;
                    default:
                        break;
                }

                Product = product;
            }
            else
            {
                throw new ArgumentException(nameof(parameter));
            }

            IsBusy = false;
            return Task.CompletedTask;
        }

        private async Task ExecutePurchaseItemCommand()
        {
            if (Product == null)
            {
                throw new InvalidOperationException("product is null");
            }

            if (Product != null && Product.Purchased)
            {
                await dialogService.ShowAlertAsync(AppResources.Hint_Content_AlreadyPurchased, AppResources.Hint_Caption_AlreadyPurchased);
                return;
            }

            IsBusy = true;
            try
            {
                var (success, error, cancelledByUser) = await inAppBillingService.PurchaseProductAsync(Product.Package);

                if (!success && !cancelledByUser && !string.IsNullOrWhiteSpace(error))
                {
                    await dialogService.ShowAlertAsync(error ?? AppResources.Error_Content_General, AppResources.Error_Caption);
                }

                if (success && !cancelledByUser)
                {
                    Product.Purchased = true;
                    featureRegistry.Update(Product.ProductId, success);
                    await NavigationService.GoBackAsync();
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
        #endregion Methods
    }
}
