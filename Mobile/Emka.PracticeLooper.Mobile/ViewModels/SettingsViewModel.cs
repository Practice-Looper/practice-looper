// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Plugin.InAppBilling;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SettingsViewModel : ViewModelBase
    {
        #region Fields

        private readonly IConfigurationService configService;
        private readonly ILogger logger;
        private readonly IDialogService dialogService;
        private readonly IAppTracker appTracker;
        private readonly IInAppBillingVerifyPurchase purchaseVerifier;
        private Command purchaseItemCommand;
        private bool isBusy;
        private bool hasProducts;
        #endregion

        #region Ctor

        public SettingsViewModel(IConfigurationService configService, ILogger logger, IDialogService dialogService, IAppTracker appTracker, IInAppBillingVerifyPurchase purchaseVerifier)
        {
            this.configService = configService;
            this.logger = logger;
            this.dialogService = dialogService;
            this.appTracker = appTracker;
            this.purchaseVerifier = purchaseVerifier;
            Products = new ObservableCollection<InAppBillingProductViewModel>();
            HasProducts = true;
        }
        #endregion

        #region Properties
        public Command PurchaseItemCommand => purchaseItemCommand ?? (purchaseItemCommand = new Command(async o => await ExecutePurchaseItemCommand(o)));
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
            await GetInAppItems();
        }

        private async Task GetInAppItems()
        {
            IsBusy = true;
            var billing = CrossInAppBilling.Current;
            try
            {
                var productIds = new[] { PreferenceKeys.PremiumGeneral};
                //You must connect
                var connected = await billing.ConnectAsync(ItemType.InAppPurchase);

                if (!connected)
                {
                    await logger.LogErrorAsync(new Exception($"Could not connect to store {Xamarin.Essentials.DeviceInfo.Platform}"));
                    await dialogService.ShowAlertAsync("Oops, could not connect store! Please try again later.");
                    return;
                }

                //check purchases
                var items = await billing.GetProductInfoAsync(ItemType.InAppPurchase, productIds);

                foreach (var item in items)
                {
                    var vm = new InAppBillingProductViewModel(item)
                    {
                        Purchased = await WasItemPurchased(item.ProductId)
                    };
                    Products.Add(vm);
                    if (vm.Purchased)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                         {
                             Preferences.Set(PreferenceKeys.PremiumGeneral, true);
                             configService.SetValue(PreferenceKeys.PremiumGeneral, true);
                         });
                    }
                }
            }
            catch (InAppBillingPurchaseException pEx)
            {
                var message = GetErrorMessage(pEx.PurchaseError);
                //Decide if it is an error we care about
                if (string.IsNullOrWhiteSpace(message))
                    return;

                await dialogService.ShowAlertAsync(message);
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync("Something went wrong, please try again.");
            }
            finally
            {
                await billing.DisconnectAsync();
                IsBusy = false;
                HasProducts = Products.Any();
            }
        }

        private async Task ExecutePurchaseItemCommand(object o)
        {
            if (o is InAppBillingProductViewModel product)
            {
                if (product.Purchased)
                {
                    await dialogService.ShowAlertAsync("Already purchased", "You already purchased this upgrade.");
                    return;
                }

                IsBusy = true;
                var billing = CrossInAppBilling.Current;
                try
                {
                    var connected = await billing.ConnectAsync(ItemType.InAppPurchase);
                    if (!connected)
                    {
                        //we are offline or can't connect, don't try to purchase
                        await dialogService.ShowAlertAsync("Oops, could not connect store! Please try again later.");
                        return;
                    }

                    //check purchases
                    var payload = Guid.NewGuid().ToString();
                    var purchase = await billing.PurchaseAsync(product.Model.ProductId, ItemType.InAppPurchase, payload, purchaseVerifier);

                    //possibility that a null came through.
                    if (purchase == null)
                    {
                        await dialogService.ShowAlertAsync("Oops, purchase failed! Please try again later.");
                    }
                    else if (purchase.State == PurchaseState.Purchased)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Preferences.Set(PreferenceKeys.PremiumGeneral, true);
                            configService.SetValue(PreferenceKeys.PremiumGeneral, true);
                        });
                        var purchasedVm = Products.FirstOrDefault(p => p.Model.ProductId == product.Model.ProductId);
                        purchasedVm.Purchased = true;
                    }
                }
                catch (InAppBillingPurchaseException pEx)
                {
                    //Billing Exception handle this based on the type
                    var message = GetErrorMessage(pEx.PurchaseError);
                    //Decide if it is an error we care about
                    if (string.IsNullOrWhiteSpace(message))
                        return;

                    await dialogService.ShowAlertAsync(message);
                }
                catch (Exception ex)
                {
                    await logger.LogErrorAsync(ex);
                    await dialogService.ShowAlertAsync("Something went wrong, please try again.");
                }
                finally
                {
                    await billing.DisconnectAsync();
                    IsBusy = false;
                }
            }
        }

        public async Task<bool> WasItemPurchased(string productId)
        {
            var billing = CrossInAppBilling.Current;
            try
            {
                if (Preferences.Get(productId, false))
                {
                    return true;
                }

                var connected = await billing.ConnectAsync(ItemType.InAppPurchase);

                if (!connected)
                {
                    await dialogService.ShowAlertAsync("Oops, could not connect store! Please try again later.");
                    return false;
                }

                //check purchases
                var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);

                //check for null just incase
                if (purchases?.Any(p => p.ProductId == productId) ?? false)
                {
                    return true;
                }
                else
                {
                    //no purchases found
                    return false;
                }
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                //Billing Exception handle this based on the type
                var message = GetErrorMessage(purchaseEx.PurchaseError);
                //Decide if it is an error we care about
                if (string.IsNullOrWhiteSpace(message))
                    return false;

                await dialogService.ShowAlertAsync(message);
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync("Something went wrong, please try again.");
            }
            finally
            {
                await billing.DisconnectAsync();
            }

            return false;
        }

        private string GetErrorMessage(PurchaseError error)
        {
            var message = string.Empty;
            switch (error)
            {
                case PurchaseError.AppStoreUnavailable:
                    message = "Currently the app store seems to be unavailble. Try again later.";
                    break;
                case PurchaseError.BillingUnavailable:
                    message = "Billing seems to be unavailable, please try again later.";
                    break;
                case PurchaseError.PaymentInvalid:
                    message = "Payment seems to be invalid, please try again.";
                    break;
                case PurchaseError.PaymentNotAllowed:
                    message = "Payment does not seem to be enabled/allowed, please try again.";
                    break;

                case PurchaseError.InvalidProduct:
                case PurchaseError.DeveloperError:
                case PurchaseError.ItemUnavailable:
                case PurchaseError.GeneralError:
                case PurchaseError.ProductRequestFailed:

                case PurchaseError.ServiceUnavailable:
                    message = "Something went wrong, please contact the support.";
                    break;
                case PurchaseError.UserCancelled:
                    break;
                case PurchaseError.RestoreFailed:
                    message = "Failed to restore purchase, please try again.";
                    break;
                case PurchaseError.AlreadyOwned:
                    break;
                case PurchaseError.NotOwned:
                    break;
            }

            return message;
        }
        #endregion
    }
}
