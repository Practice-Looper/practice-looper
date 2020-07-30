#!/usr/bin/env bash

echo "******************************************************************************"
echo "*************** Custom Post Clone Script -- iOS Edition **********************"
echo "******************************************************************************"

echo "************************* Build secrets.json iOS *****************************"

secrets_location=$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka3.PracticeLooper.Config/secrets.json
echo "Write file to $secrets_location"

echo "{
        AppCenterAndroidLite:\"$AppCenterAndroidLite\", \n
        AppCenterAndroidPremium:\"$AppCenterAndroidPremium\", \n
        AppCenterIosLite:\"$AppCenterIosLite\", \n
        AppCenterIosPremium:\"$AppCenterIosPremium\", \n
        SpotifyApiLimit: $SpotifyApiLimit, \n
        SpotifyClientId:\"$SpotifyClientId\", \n
        SpotifyClientSecret:\"$SpotifyClientSecret\", \n
        SpotifyClientScopes: \"$SpotifyClientScopes\", \n
        SpotifyClientApiUri:\"$SpotifyClientApiUri\", \n
        SpotifyClientAuthUri:\"$SpotifyClientAuthUri\", \n
        SpotifyClientRedirectUri:\"$SpotifyClientRedirectUri\", \n
        SpotifyClientTokenUri:\"$SpotifyClientTokenUri\", \n
        SpotifyClientUseNativeUi: $SpotifyClientUseNativeUi, \n
        SpotifyClientRequestCode: $SpotifyClientRequestCode, \n
        AdmobIosTopBannerAdId:\"$AdmobIosTopBannerAdId\", \n
        AdmobIosInterstitialProjectAdId:\"$AdmobIosInterstitialProjectAdId\", \n
        AdmobAndroidAppId:\"$AdmobAndroidAppId\", \n
        AdmobAndroidTopBannerAdId: \"$AdmobAndroidTopBannerAdId\", \n
        AdmobAndroidInterstitialProjectAdId:\"$AdmobAndroidInterstitialProjectAdId\", \n
        DbName:\"$DbName\", \n
        InAppIosPremiumGeneral:\"$InAppIosPremiumGeneral\", \n
        InAppAndroidPremiumGeneral:\"$InAppAndroidPremiumGeneral\", \n
        SyncFusionLicenseKey:\"$SyncFusionLicenseKey\", \n
        AdmobIosAppId:\"$AdmobIosAppId\", \n
        PublicKey1:\"$PublicKey1\", \n
        PublicKey2:\"$PublicKey2\", \n
        PublicKey3:\"$PublicKey3\" \n
      }" > $secrets_location

echo "**************************** secrets.json iOS *******************************"
cat $secrets_location
