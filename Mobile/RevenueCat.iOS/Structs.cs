using System;
using ObjCRuntime;

namespace RevenueCat.iOS.Purchases
{
	[Native]
	public enum RCAttributionNetwork : long
	{
		AppleSearchAds = 0,
		Adjust,
		AppsFlyer,
		Branch,
		Tenjin,
		Facebook,
		MParticle
	}

	[Native]
	public enum RCIntroEligibilityStatus : long
	{
		Unknown = 0,
		Ineligible,
		Eligible
	}

	[Native]
	public enum RCPackageType : long
	{
		Unknown = -2,
		Custom,
		Lifetime,
		Annual,
		SixMonth,
		ThreeMonth,
		TwoMonth,
		Monthly,
		Weekly
	}

	[Native]
	public enum RCPurchasesErrorCode : long
	{
		Unknown = 0,
		PurchaseCancelled,
		StoreProblem,
		PurchaseNotAllowed,
		PurchaseInvalid,
		ProductNotAvailableForPurchase,
		ProductAlreadyPurchased,
		ReceiptAlreadyInUse,
		InvalidReceipt,
		MissingReceiptFile,
		Network,
		InvalidCredentials,
		UnexpectedBackendResponse,
		ReceiptInUseByOtherSubscriber,
		InvalidAppUserId,
		OperationAlreadyInProgress,
		UnknownBackend,
		InvalidAppleSubscriptionKey,
		Ineligible,
		InsufficientPermissions,
		PaymentPending,
		InvalidSubscriberAttributes
	}

	[Native]
	public enum RCBackendErrorCode : long
	{
		InvalidPlatform = 7000,
		StoreProblem = 7101,
		CannotTransferPurchase = 7102,
		InvalidReceiptToken = 7103,
		InvalidAppStoreSharedSecret = 7104,
		InvalidPaymentModeOrIntroPriceNotProvided = 7105,
		ProductIdForGoogleReceiptNotProvided = 7106,
		InvalidPlayStoreCredentials = 7107,
		EmptyAppUserId = 7220,
		InvalidAuthToken = 7224,
		InvalidAPIKey = 7225,
		PlayStoreQuotaExceeded = 7229,
		PlayStoreInvalidPackageName = 7230,
		PlayStoreGenericError = 7231,
		UserIneligibleForPromoOffer = 7232,
		InvalidAppleSubscriptionKey = 7234,
		InvalidSubscriberAttributes = 7263,
		InvalidSubscriberAttributesBody = 7264
	}

	[Native]
	public enum RCStore : long
	{
		AppStore = 0,
		MacAppStore,
		PlayStore,
		Stripe,
		Promotional,
		UnknownStore
	}

	[Native]
	public enum RCPeriodType : long
	{
		Normal = 0,
		Intro,
		Trial
	}
}
