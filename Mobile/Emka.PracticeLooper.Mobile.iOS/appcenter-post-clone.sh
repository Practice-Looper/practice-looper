#!/usr/bin/env bash

echo "******************************************************************************"
echo "*************** Custom Post Clone Script -- iOS Edition **********************"
echo "******************************************************************************"

echo "************************* Build secrets.json iOS *****************************"

secrets_location=$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka3.PracticeLooper.Config/secrets.json
credentials_location=$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka3.PracticeLooper.Config/credentials.json
echo "Write file to $secrets_location"
echo "" > $credentials_location

echo "{
    AppCenterAndroidLite: \"$AppCenterAndroidLite\",
	AppCenterAndroidPremium: \"$AppCenterAndroidPremium\",
	AppCenterIosLite: \"$AppCenterIosLite\",
	AppCenterIosPremium: \"$AppCenterIosPremium\",
	SpotifyApiLimit: $SpotifyApiLimit,
	SpotifyClientId: \"$SpotifyClientId\",
	SpotifyClientSecret: \"$SpotifyClientSecret\",
	SpotifyClientScopes: \"$SpotifyClientScopes\",
	SpotifyClientApiUri: \"$SpotifyClientApiUri\",
	SpotifyClientRedirectUri: \"$SpotifyClientRedirectUri\",
	SpotifyClientTokenUri: \"$SpotifyClientTokenUri\",
	SpotifyClientAuthUri: \"$SpotifyClientAuthUri\",
	SpotifyClientAccountApiUri: \"$SpotifyClientAccountApiUri\",
	SpotifyClientRequestCode: $SpotifyClientRequestCode,
	AdmobIosTopBannerAdId: \"$AdmobIosTopBannerAdId\",
	AdmobIosInterstitialProjectAdId: \"$AdmobIosInterstitialProjectAdId\",
	AdmobAndroidAppId: \"$AdmobAndroidAppId\",
	AdmobAndroidTopBannerAdId: \"$AdmobAndroidTopBannerAdId\",
	AdmobAndroidInterstitialProjectAdId: \"$AdmobAndroidInterstitialProjectAdId\",
	DbName: \"$DbName\",
	PurchaseItems: {
		IosPremiumLifetime: \"$IosPremiumLifetime\",
    	AndroidPremiumLifetime: \"$AndroidPremiumLifetime\"
	},
	SyncFusionLicenseKey: \"$SyncFusionLicenseKey\",
	AdmobIosAppId: \"$AdmobIosAppId\",
	RevenueCatPubKey: \"$RevenueCatPubKey\",
	ThirdPartyComponentsUrl: \"$ThirdPartyComponentsUrl\",
	DataPrivacyUrl: \"$DataPrivacyUrl\",
	SpotifyConnectionTimeOut: \"$SpotifyConnectionTimeOut\"
}" > $secrets_location

echo "**************************** secrets.json iOS *******************************"
cat $secrets_location
