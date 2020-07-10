#!/usr/bin/env bash

echo "******************************************************************************"
echo "************* Custom Post Clone Script -- Android Edition ********************"
echo "******************************************************************************"

echo "********************** Build secrets.json Android ****************************"

secrets_location=$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka3.PracticeLooper.Config/secrets.json
echo "Write file to $secrets_location"

echo "{SpotifyClientSecret:\"$Secret_SpotifyClientSecret\", SpotifyClientAuthUri:\"$Secret_SpotifyClientAuthUri\", SpotifyClientTokenUri:\"$Secret_SpotifyClientTokenUri\", SpotifyClientUseNativeUi: $Secret_SpotifyClientUseNativeUi, AdmobIosAppId:\"$Secret_AdmobIosAppId\", AdmobIosTopBannerAdId:\"$Secret_AdmobIosTopBannerAdId\", AdmobIosInterstitialProjectAdId:\"$Secret_AdmobIosInterstitialProjectAdId\", AppCenterIos:\"$Secret_AppCenterIos\", , SpotifyApiLimit: $Secret_SpotifyApiLimit, SpotifyClientId:\"$Secret_SpotifyClientId\", SpotifyClientScopes:\"$Secret_SpotifyClientScopes\", SpotifyClientApiUri:\"$Secret_SpotifyClientApiUri\", SpotifyClientRedirectUri:\"$Secret_SpotifyClientRedirectUri\", SpotifyClientRequestCode: $Secret_SpotifyClientRequestCode, AdmobAndroidAppId:\"$Secret_AdmobAndroidAppId\", AppCenterAndroid:\"$Secret_AppCenterAndroid\", PublicKey1:\"$Secret_PublicKey1\", PublicKey2:\"$Secret_PublicKey2\", PublicKey3:\"$Secret_PublicKey3\", AdmobAndroidTopBanneAdId:\"$Secret_AdmobAndroidTopBanneAdId\", AdmobAndroidInterstitialProjectAdId:\"$Secret_AdmobAndroidInterstitialProjectAdId\", DbName:\"$Secret_DbName\", InAppIosPremiumGeneral:\"$Secret_InAppIosPremiumGeneral\", InAppAndroidPremiumGeneral:\"$Secret_InAppAndroidPremiumGeneral\", SyncFusionLicenseKey:\"$Secret_SyncFusionLicenseKey\"}" > $secrets_location

echo "************************** secrets.json Android *****************************"
cat $secrets_location