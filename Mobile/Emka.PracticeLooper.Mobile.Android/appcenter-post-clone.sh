#!/usr/bin/env bash

echo "******************************************************************************"
echo "************* Custom Post Clone Script -- Android Edition ********************"
echo "******************************************************************************"

echo "********************** Build secrets.json Android ****************************"

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


echo "************************** secrets.json Android *****************************"
cat $secrets_location

