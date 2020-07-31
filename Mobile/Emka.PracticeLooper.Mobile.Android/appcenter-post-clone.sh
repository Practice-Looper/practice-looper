#!/usr/bin/env bash

echo "******************************************************************************"
echo "************* Custom Post Clone Script -- Android Edition ********************"
echo "******************************************************************************"

echo "********************** Build secrets.json Android ****************************"

secrets_location=$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka3.PracticeLooper.Config/secrets.json
echo "Write file to $secrets_location"

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
	PublicKey3: \"$PublicKey3\" 
}" > $secrets_location


echo "************************** secrets.json Android *****************************"
cat $secrets_location

echo "************************ Install custom .NET Core ***************************"
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version $netcoreversion --install-dir "$AGENT_TOOLSDIRECTORY/dotnet"
