// Copyright (C) - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.BillingClient.Api;
using Android.Runtime;
using Com.Revenuecat.Purchases;
using Com.Revenuecat.Purchases.Interfaces;
using Emka.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    [Preserve(AllMembers = true)]
    public class InAppBillingService : Java.Lang.Object, IInAppBillingService, IReceiveOfferingsListener, ICallback, IMakePurchaseListener, IReceivePurchaserInfoListener
    {
        #region Fields
          
        static AutoResetEvent fetchProductsEvent;
        static AutoResetEvent purchaseProductEvent;
        static AutoResetEvent restorePurchasesEvent;
        static AutoResetEvent isSupportedEvent;
        private readonly IConfigurationService configurationService;
        private readonly ILogger logger;
        private readonly IStringLocalizer stringLocalizer;
        private Offerings offerings;
        private (bool Success, string Error, bool UserCancelled) purchaseResult = (false, string.Empty, false);
        private (bool success, string reason) restoreResult = (false, null);
        private bool isBillingSupported = true;
        #endregion

        #region Ctor

        public InAppBillingService(IConfigurationService configurationService, ILogger logger, IStringLocalizer stringLocalizer)
        {
            Products = new Dictionary<string, InAppPurchaseProduct>();
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.stringLocalizer = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
        }
        #endregion


        #region Properties

        public IDictionary<string, InAppPurchaseProduct> Products { get; private set; }

        public void Init()
        {
            var key = configurationService.GetValue("RevenueCatPubKey");
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            Purchases.Configure(Android.App.Application.Context, key);
            StartFetchingOfferings();
        }

        public void StartFetchingOfferings()
        {
            fetchProductsEvent = new AutoResetEvent(false);
            Purchases.SharedInstance.GetOfferings(this);
        }

        public async Task FetchOfferingsAsync()
        {
            StartFetchingOfferings();
            await Task.Run(fetchProductsEvent.WaitOne);
        }

        void IReceiveOfferingsListener.OnReceived(Offerings paramReceiveOfferingsOnReceived)
        {
            offerings = paramReceiveOfferingsOnReceived;
            Offering defaultOffering = offerings?.Current;
            Package package = defaultOffering.Lifetime;

            if (package != null && !Products.ContainsKey(package.Identifier))
            {
                Products.Add(package.Identifier, new InAppPurchaseProduct
                {
                    CurrencyCode = package.Product.PriceCurrencyCode,
                    Description = package.Product.Description,
                    LocalizedIntroductoryPrice = package.Product?.IntroductoryPrice?.ToString(),
                    LocalizedPrice = package.Product.Price,
                    Name = package.Product.Title,
                    ProductId = package.Product.Sku,
                    Package = package
                });
            }

            fetchProductsEvent?.Set();
        }

        void IReceiveOfferingsListener.OnError(PurchasesError error)
        {
            fetchProductsEvent?.Set();
            throw new Exception(error.Message);
        }

        public async Task<(bool Success, string Error, bool UserCancelled)> PurchaseProductAsync(object package)
        {
            purchaseProductEvent = new AutoResetEvent(false);
            isSupportedEvent = new AutoResetEvent(false);

            Purchases.IsBillingSupported(Android.App.Application.Context, this);
            
            await Task.Run(() => { isSupportedEvent.WaitOne(); });

            if (!isBillingSupported) {
                return purchaseResult;
            }

            if (package is Package inPackage)
            {
                Purchases.SharedInstance.PurchasePackage(Xamarin.Essentials.Platform.CurrentActivity, inPackage, this);

                await Task.Run(() => { purchaseProductEvent.WaitOne(); });
                return purchaseResult;
            }

            throw new ArgumentNullException(nameof(inPackage));
        }

        async void IMakePurchaseListener.OnError(PurchasesError error, bool canceldByUser)
        {
            // canceld by user
            if (canceldByUser)
            {
                purchaseResult = (false, error.Message, true);
            }
            // not cancelled by user but something went wrong
            else
            {
                logger.LogError(new Exception(error.Message), new Dictionary<string, string>
                        {
                            { "ErrorCodeKey", error.Code.ToString() },
                            { "Message", error.Message },
                            { "Underlying Error", error.UnderlyingErrorMessage?.ToString() }
                        });

                var (handled, reason) = await HandlePurchaseErrorAsync(error);
                purchaseResult = (handled, reason, false);
            }

            purchaseProductEvent.Set();

        }

        void IMakePurchaseListener.OnCompleted(Purchase purchase, PurchaserInfo purchaserInfo)
        {
            // purchase succeeded
            if (purchaserInfo.Entitlements.All["premium"].IsActive)
            {
                purchaseResult = (true, string.Empty, false);
            }

            purchaseProductEvent.Set();
        }


        void ICallback.OnReceived(Java.Lang.Object isSupported)
        {
            isBillingSupported = (bool) isSupported;

            if (!isBillingSupported) {
                purchaseResult = (false, "No purchases allowed", false);
            }
            
            isSupportedEvent.Set();
        }

        public async Task<(bool Success, string Error)> RestorePurchasesAsync()
        {
            restorePurchasesEvent = new AutoResetEvent(false);

            Purchases.SharedInstance.RestorePurchases(this);

            await Task.Run(() => { restorePurchasesEvent.WaitOne(); });
            return restoreResult;
        }

        void IReceivePurchaserInfoListener.OnError(PurchasesError error)
        {
            logger.LogError(new Exception(error.Message), new Dictionary<string, string>
                        {
                            { "ErrorCodeKey", error.Code.ToString() },
                            { "Message", error.Message },
                            { "Underlying Error", error.UnderlyingErrorMessage?.ToString() }
                        });

            var (handled, restoreError) = HandleRestoreErrors(error);
            restoreResult = (handled, restoreError);
            restorePurchasesEvent.Set();
        }

        void IReceivePurchaserInfoListener.OnReceived(PurchaserInfo purchaserInfo)
        {
            if (purchaserInfo.Entitlements.All["premium"].IsActive)
            {
                restoreResult = (true, null);
            }

            restorePurchasesEvent.Set();
        }

        private (bool Handled, string Error) HandleCommonErrors(PurchasesErrorCode errorCode)
        {
            switch (errorCode.ToString())
            {
                case nameof(PurchasesErrorCode.UnknownBackendError):
                case nameof(PurchasesErrorCode.InvalidAppUserIdError):
                case nameof(PurchasesErrorCode.UnknownError):
                case nameof(PurchasesErrorCode.UnexpectedBackendResponseError):
                case nameof(PurchasesErrorCode.InvalidCredentialsError):
                case nameof(PurchasesErrorCode.IneligibleError):
                case nameof(PurchasesErrorCode.ReceiptInUseByOtherSubscriberError):
                case nameof(PurchasesErrorCode.InvalidSubscriberAttributesError):
                case nameof(PurchasesErrorCode.InvalidAppleSubscriptionKeyError):
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_General"));
                case nameof(PurchasesErrorCode.StoreProblemError):
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_CouldNotConnectToStore"));
                case nameof(PurchasesErrorCode.ProductAlreadyPurchasedError):
                    return (true, stringLocalizer.GetLocalizedString("Hint_Content_AlreadyPurchased"));
                case nameof(PurchasesErrorCode.NetworkError):
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_CouldNotConnectToStore"));
                case nameof(PurchasesErrorCode.OperationAlreadyInProgressError):
                    // Dein Kauf wird gerade getätigt, bitte warte einen Moment.
                    return (false, "Your purchase is being processed, please wait a moment.");
                default:
                    logger.LogError(new Exception($"unknown error code {errorCode}"));
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_General"));
            }
        }

        private async Task<(bool Handled, string Error)> HandlePurchaseErrorAsync(PurchasesError error)
        {
            switch (error.Code.ToString())
            {
                case nameof(PurchasesErrorCode.PurchaseInvalidError):
                case nameof(PurchasesErrorCode.InvalidReceiptError):
                case nameof(PurchasesErrorCode.MissingReceiptFileError):
                    // this errors should not occur!
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_General"));
                case nameof(PurchasesErrorCode.ProductNotAvailableForPurchaseError):
                    // Das Produkt ist nicht zum Kauf verfügbar.
                    return (false, "The product is not available for purchase.");
                case nameof(PurchasesErrorCode.PaymentPendingError):
                    // Befolge bitte die Anweisungen von Apple, um den Kauf abzuschließen.
                    return (false, "Please follow Apple's instructions to complete the purchase.");
                case nameof(PurchasesErrorCode.InsufficientPermissionsError):
                    // Dieses Gerät verfügt nicht über ausreichende Berechtigungen, um In-App-Käufe zu tätigen.
                    return (false, "This device does not have sufficient permissions to make in-app purchases.");
                case nameof(PurchasesErrorCode.PurchaseCancelledError):
                case nameof(PurchasesErrorCode.ReceiptAlreadyInUseError):
                    // try to restore and show error if restore failed!
                    var (success, restoreError) = await RestorePurchasesAsync();
                    return (success, stringLocalizer.GetLocalizedString(restoreError));
                case nameof(PurchasesErrorCode.PurchaseNotAllowedError):
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_PaymentNotAllowed"));
                default:
                    return HandleCommonErrors(error.Code);
            }
        }

        private (bool Handled, string Error) HandleRestoreErrors(PurchasesError error)
        {
            if (error == null)
            {
                return (true, null);
            }

            switch (error.Code.ToString())
            {
                case nameof(PurchasesErrorCode.InvalidReceiptError):
                case nameof(PurchasesErrorCode.MissingReceiptFileError):
                    // this error shoudl not occur!
                    return (true, stringLocalizer.GetLocalizedString("Error_Content_General"));
                default:
                    return HandleCommonErrors(error.Code);
            }
        }
        #endregion
    }
}
