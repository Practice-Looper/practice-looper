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
using System.Linq;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    [Preserve(AllMembers = true)]
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
        private (bool Success, string Error) fetchOfferingsResult;
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

        public bool Initialized { get; private set; }

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
#if DEBUG
            RCPurchases.DebugLogsEnabled = true;
#endif
            try
            {
                RCPurchases.ConfigureWithAPIKey(key);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                Initialized = false;
                return;
            }

            Initialized = true;
        }

        public async Task<(bool Success, string Error)> FetchOfferingsAsync()
        {
            StartFetchingOfferings();
            await Task.Run(fetchProductsEvent.WaitOne);
            return fetchOfferingsResult;
        }

        public void StartFetchingOfferings()
        {
            fetchProductsEvent = new AutoResetEvent(false);
            RCPurchases.SharedPurchases.OfferingsWithCompletionBlock((o, e) =>
            {
                if (e != null)
                {
                    logger.LogError(new Exception(e.Description));
                    fetchOfferingsResult = (false, stringLocalizer.GetLocalizedString("SettingsView_NoUpgradesDescription"));
                }

                offerings = o;
                RCOffering defaultOffering = offerings?.Current;
                RCPackage package = defaultOffering.Lifetime;

                if (package != null && !Products.ContainsKey(package.Identifier))
                {
                    bool hasBeenPurchased;
                    bool.TryParse(configurationService.GetSecureValue(package.Product.ProductIdentifier), out hasBeenPurchased);
                    Products.Add(package.Identifier, new InAppPurchaseProduct
                    {
                        CurrencyCode = package.Product.PriceLocale.CurrencyCode,
                        Description = stringLocalizer.GetLocalizedString(package.Product.ProductIdentifier),
                        LocalizedIntroductoryPrice = package.Product?.IntroductoryPrice?.Price?.ToString(),
                        LocalizedPrice = package.LocalizedPriceString,
                        Name = package.Product.LocalizedTitle,
                        ProductId = package.Product.ProductIdentifier,
                        Package = package,
                        Purchased = hasBeenPurchased
                    });
                }

                fetchOfferingsResult = (true, null);
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
                    if (!userCancelled && error == null && purchaserInfo.Entitlements.All["Premium"] is RCEntitlementInfo info && info.IsActive)
                    {
                        configurationService.SetSecureValue(info.ProductIdentifier, info.IsActive.ToString());
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
                            { "ErrorCodeKey", err.UserInfo[Constants.RCReadableErrorCodeKey]?.ToString() },
                            { "Message", err.Description },
                            { "Underlying Error", err.UserInfo["NSUnderlyingErrorKey"]?.ToString() }
                        });

                    var (handled, restoreError) = HandleRestoreErrors(err);
                    result = (handled, restoreError);
                }

                if (error == null && purchaserInfo.Entitlements.All.Count > 0)
                {
                    foreach (var entitlement in purchaserInfo.Entitlements.All.Values)
                    {
                        var product = Products?.Values?.FirstOrDefault(p => p.ProductId == entitlement.ProductIdentifier);
                        if (product != null)
                        {
                            product.Purchased = entitlement.IsActive;
                            configurationService.SetSecureValue(entitlement.ProductIdentifier, entitlement.IsActive.ToString());
                        }
                    }
                }

                result = (true, null);
                restorePurchasesEvent.Set();
            });

            await Task.Run(() => restorePurchasesEvent.WaitOne());
            return result;
        }

        private (bool Handled, string Error) HandleCommonErrors(RCPurchasesErrorCode errorCode)
        {
            switch (errorCode)
            {
                case RCPurchasesErrorCode.UnknownBackend:
                case RCPurchasesErrorCode.InvalidAppUserId:
                case RCPurchasesErrorCode.Unknown:
                case RCPurchasesErrorCode.UnexpectedBackendResponse:
                case RCPurchasesErrorCode.InvalidCredentials:
                case RCPurchasesErrorCode.Ineligible:
                case RCPurchasesErrorCode.ReceiptInUseByOtherSubscriber:
                case RCPurchasesErrorCode.InvalidSubscriberAttributes:
                case RCPurchasesErrorCode.InvalidAppleSubscriptionKey:
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_General"));
                case RCPurchasesErrorCode.StoreProblem:
                    // return empty string, since this case doesn't needs to be handled
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_CouldNotConnectToStore"));
                case RCPurchasesErrorCode.ProductAlreadyPurchased:
                    return (true, stringLocalizer.GetLocalizedString("Hint_Content_AlreadyPurchased"));
                case RCPurchasesErrorCode.Network:
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_CouldNotConnectToStore"));
                case RCPurchasesErrorCode.OperationAlreadyInProgress:
                    // Dein Kauf wird gerade getätigt, bitte warte einen Moment.
                    return (false, "Your purchase is being processed, please wait a moment.");
                default:
                    logger.LogError(new Exception($"unknown error code {errorCode}"));
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_General"));
            }
        }

        private async Task<(bool Handled, string Error)> HandlePurchaseError(NSError error)
        {
            var code = error.UserInfo[Constants.RCReadableErrorCodeKey]?.ToString()?.Replace("_", "");
            var purchasesError = Enum.Parse<RCPurchasesErrorCode>(code, true);
            switch (purchasesError)
            {
                case RCPurchasesErrorCode.PurchaseInvalid:
                case RCPurchasesErrorCode.InvalidReceipt:
                case RCPurchasesErrorCode.MissingReceiptFile:
                    // this errors should not occur!
                    return (false, stringLocalizer.GetLocalizedString("Error_Content_General"));
                case RCPurchasesErrorCode.ProductNotAvailableForPurchase:
                    return (false, stringLocalizer.GetLocalizedString("InAppBillingError_ProductNotAvaliable"));
                case RCPurchasesErrorCode.PaymentPending:
                    return (false, stringLocalizer.GetLocalizedString("InAppBillingError_PaymentPending"));
                case RCPurchasesErrorCode.InsufficientPermissions:
                    return (false, stringLocalizer.GetLocalizedString("InAppBillingError_InsufficientPermissions"));
                case RCPurchasesErrorCode.PurchaseCancelled:
                case RCPurchasesErrorCode.ReceiptAlreadyInUse:
                    // try to restore and show error if restore failed!
                    var (success, restoreError) = await RestorePurchasesAsync();
                    return (success, stringLocalizer.GetLocalizedString(restoreError));
                case RCPurchasesErrorCode.PurchaseNotAllowed:
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

            var code = error.UserInfo[Constants.RCReadableErrorCodeKey]?.ToString()?.Replace("_", "");
            var purchasesError = Enum.Parse<RCPurchasesErrorCode>(code, true);
            switch (purchasesError)
            {
                case RCPurchasesErrorCode.InvalidReceipt:
                case RCPurchasesErrorCode.MissingReceiptFile:
                    // this error shoudl not occur!
                    return (true, stringLocalizer.GetLocalizedString("Error_Content_General"));
                default:
                    return HandleCommonErrors(purchasesError);
            }
        }
        #endregion
    }
}
