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
	SpotifyClientAuthUri: \"$SpotifyClientAuthUri\",
	SpotifyClientRedirectUri: \"$SpotifyClientRedirectUri\",
	SpotifyClientTokenUri: \"$SpotifyClientTokenUri\",
	SpotifyClientUseNativeUi: $SpotifyClientUseNativeUi,
	SpotifyClientRequestCode: $SpotifyClientRequestCode,
	AdmobIosTopBannerAdId: \"$AdmobIosTopBannerAdId\",
	AdmobIosInterstitialProjectAdId: \"$AdmobIosInterstitialProjectAdId\",
	AdmobAndroidAppId: \"$AdmobAndroidAppId\",
	AdmobAndroidTopBannerAdId: \"$AdmobAndroidTopBannerAdId\",
	AdmobAndroidInterstitialProjectAdId: \"$AdmobAndroidInterstitialProjectAdId\",
	DbName: \"$DbName\",
	InAppIosPremiumGeneral: \"$InAppIosPremiumGeneral\",
	InAppAndroidPremiumGeneral: \"$InAppAndroidPremiumGeneral\",
	SyncFusionLicenseKey: \"$SyncFusionLicenseKey\",
	AdmobIosAppId: \"$AdmobIosAppId\",
	PublicKey1: \"$PublicKey1\",
	PublicKey2: \"$PublicKey2\",
	PublicKey3: \"$PublicKey3\",
	RevenueCatPubKey: \"$RevenueCatPubKey\",
	ThirdPartyComponentsUrl: \"$ThirdPartyComponentsUrl\",
	DataPrivacyUrl: \"$DataPrivacyUrl\"
	SpotifyConnectionTimeOut: \"$SpotifyConnectionTimeOut\"
}" > $secrets_location

echo "**************************** secrets.json iOS *******************************"
cat $secrets_location
