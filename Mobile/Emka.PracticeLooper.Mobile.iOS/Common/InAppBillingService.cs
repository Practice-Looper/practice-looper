// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using Emka3.PracticeLooper.Services.Contracts.Common;
using RevenueCat.iOS.Purchases;
using System.Collections.Generic;
using Emka3.PracticeLooper.Model.Common;
using System.Threading.Tasks;
using System.Threading;
using Emka3.PracticeLooper.Config.Contracts;
using Foundation;
using Emka.PracticeLooper.Services.Contracts.Common;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class InAppBillingService : IInAppBillingService
    {
        #region Fields

        static AutoResetEvent fetchProductsEvent;
        static AutoResetEvent purchaseProductEvent;
        static AutoResetEvent restorePurchasesEvent;
        private readonly IConfigurationService configurationService;
        private readonly ILogger logger;
        private readonly IStringLocalizer stringLocalizer;
        private RCOfferings offerings;
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
        #endregion

        #region Methods

        public void Init()
        {
            var key = configurationService.GetValue("RevenueCatPubKey");
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            RCPurchases.ConfigureWithAPIKey(key);
            StartFetchingOfferings();
        }

        public async Task FetchOfferingsAsync()
        {
            StartFetchingOfferings();
            await Task.Run(fetchProductsEvent.WaitOne);
        }

        public void StartFetchingOfferings()
        {
            fetchProductsEvent = new AutoResetEvent(false);
            RCPurchases.SharedPurchases.OfferingsWithCompletionBlock((o, e) =>
            {
                if (e != null)
                {
                    fetchProductsEvent?.Set();
                    throw new Exception(e.Description);
                }

                offerings = o;
                RCOffering defaultOffering = offerings?.Current;
                RCPackage package = defaultOffering.Lifetime;

                if (package != null && !Products.ContainsKey(package.Identifier))
                {
                    Products.Add(package.Identifier, new InAppPurchaseProduct
                    {
                        CurrencyCode = package.Product.PriceLocale.CurrencyCode,
                        Description = package.Product.LocalizedDescription,
                        LocalizedIntroductoryPrice = package.Product?.IntroductoryPrice?.Price?.ToString(),
                        LocalizedPrice = package.LocalizedPriceString,
                        Name = package.Product.LocalizedTitle,
                        ProductId = package.Product.ProductIdentifier,
                        Package = package
                    });
                }

                fetchProductsEvent?.Set();
            });
        }

        public async Task<(bool Success, string Error, bool UserCancelled)> PurchaseProductAsync(object inPackage)
        {
            if (!RCPurchases.CanMakePayments)
            {
                return (false, "No purchases allowed", false);
            }

            purchaseProductEvent = new AutoResetEvent(false);
            (bool Success, string Error, bool UserCancelled) result = (false, null, false);
            if (inPackage is RCPackage package)
            {
                RCPurchases.SharedPurchases.PurchasePackage(package, async (transaction, purchaserInfo, error, userCancelled) =>
                {
                    // cancelled by user
                    if (userCancelled)
                    {
                        result = (false, error?.Description, true);
                    }

                    // not cancelled by user but something went wrong
                    if (!userCancelled && error is NSError err)
                    {
                        logger.LogError(new Exception(err.Description), new Dictionary<string, string>
                        {
                            { "ErrorCodeKey", err.UserInfo[Constants.RCReadableErrorCodeKey].ToString() },
                            { "Message", err.Description },
                            { "Underlying Error", err.UserInfo["NSUnderlyingErrorKey"]?.ToString() }
                        });

                        var (handled, reason) = await HandlePurchaseError(err);
                        result = (handled, reason, false);
                    }

                    // purchase succeeded
                    if (purchaserInfo.Entitlements.All["Premium"] is RCEntitlementInfo info && info.IsActive)
                    {
                        result = (true, string.Empty, false);
                    }

                    purchaseProductEvent.Set();
                });

                await Task.Run(() => { purchaseProductEvent.WaitOne(); });
                return result;
            }

            throw new ArgumentNullException(nameof(inPackage));
        }

        public async Task<(bool Success, string Error)> RestorePurchasesAsync()
        {
            (bool success, string reason) result = (false, null);
            restorePurchasesEvent = new AutoResetEvent(false);
            RCPurchases.SharedPurchases.RestoreTransactionsWithCompletionBlock((purchaserInfo, error) =>
            {
                // not cancelled by user but something went wrong
                if (error is NSError err)
                {
                    logger.LogError(new Exception(err.Description), new Dictionary<string, string>
                        {
                            { "ErrorCodeKey", err.UserInfo[Constants.RCReadableErrorCodeKey].ToString() },
                            { "Message", err.Description },
                            { "Underlying Error", err.UserInfo["NSUnderlyingErrorKey"]?.ToString() }
                        });

                    var (handled, restoreError) = HandleRestoreErrors(err);
                    result = (handled, restoreError);
                }

                // purchase succeeded
                if (purchaserInfo.Entitlements.All["premium"] is RCEntitlementInfo info && info.IsActive)
                {
                    result = (true, null);
                }

                restorePurchasesEvent.Set();
            });

            await Task.Run(() => { restorePurchasesEvent.WaitOne(); });
            return result;
        }

        private (bool Handled, string Error) HandleCommonErrors(RCPurchasesErrorCode errorCode)
        {
            switch (errorCode)
            {
                case RCPurchasesErrorCode.UnknownBackendError:
                case RCPurchasesErrorCode.InvalidAppUserIdError:
                case RCPurchasesErrorCode.UnknownError:
                case RCPurchasesErrorCode.UnexpectedBackendResponseError:
                case RCPurchasesErrorCode.InvalidCredentialsError:
                case RCPurchasesErrorCode.IneligibleError:
                case RCPurchasesErrorCode.ReceiptInUseByOtherSubscriberError:
                case RCPurchasesErrorCode.InvalidSubscriberAttributesError:
                case RCPurchasesErrorCode.InvalidAppleSubscriptionKeyError:
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_General"));
                case RCPurchasesErrorCode.StoreProblemError:
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_CouldNotConnectToStore"));
                case RCPurchasesErrorCode.ProductAlreadyPurchasedError:
                    return (true, stringLocalizer.GetLocalizedString("Hint_Content_AlreadyPurchased"));
                case RCPurchasesErrorCode.NetworkError:
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_CouldNotConnectToStore"));
                case RCPurchasesErrorCode.OperationAlreadyInProgressError:
                    // Dein Kauf wird gerade getätigt, bitte warte einen Moment.
                    return (false, "Your purchase is being processed, please wait a moment.");
                default:
                    logger.LogError(new Exception($"unknown error code {errorCode}"));
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_General"));
            }
        }

        private async Task<(bool Handled, string Error)> HandlePurchaseError(NSError error)
        {
            var purchasesError = Enum.Parse<RCPurchasesErrorCode>(error.UserInfo[Constants.RCReadableErrorCodeKey].ToString());
            switch (purchasesError)
            {
                case RCPurchasesErrorCode.PurchaseInvalidError:
                case RCPurchasesErrorCode.InvalidReceiptError:
                case RCPurchasesErrorCode.MissingReceiptFileError:
                    // this errors should not occur!
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_General"));
                case RCPurchasesErrorCode.ProductNotAvailableForPurchaseError:
                    // Das Produkt ist nicht zum Kauf verfügbar.
                    return (false, "The product is not available for purchase.");
                case RCPurchasesErrorCode.PaymentPendingError:
                    // Befolge bitte die Anweisungen von Apple, um den Kauf abzuschließen.
                    return (false, "Please follow Apple's instructions to complete the purchase.");
                case RCPurchasesErrorCode.InsufficientPermissionsError:
                    // Dieses Gerät verfügt nicht über ausreichende Berechtigungen, um In-App-Käufe zu tätigen.
                    return (false, "This device does not have sufficient permissions to make in-app purchases.");
                case RCPurchasesErrorCode.PurchaseCancelledError:
                case RCPurchasesErrorCode.ReceiptAlreadyInUseError:
                    // try to restore and show error if restore failed!
                    var (success, restoreError) = await RestorePurchasesAsync();
                    return (success, stringLocalizer.GetLocalizedString(restoreError));
                case RCPurchasesErrorCode.PurchaseNotAllowedError:
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_PaymentNotAllowed"));
                default:
                    return HandleCommonErrors(purchasesError);
            }
        }

        private (bool Handled, string Error) HandleRestoreErrors(NSError error)
        {
            if (error == null)
            {
                return (true, null);
            }

            var purchasesError = Enum.Parse<RCPurchasesErrorCode>(error.UserInfo[Constants.RCReadableErrorCodeKey].ToString());
            switch (purchasesError)
            {
                case RCPurchasesErrorCode.InvalidReceiptError:
                case RCPurchasesErrorCode.MissingReceiptFileError:
                    // this error shoudl not occur!
                    return (true, stringLocalizer.GetLocalizedString("Error_Content_General"));
                default:
                    return HandleCommonErrors(purchasesError);
            }
        }
        #endregion
    }
}
