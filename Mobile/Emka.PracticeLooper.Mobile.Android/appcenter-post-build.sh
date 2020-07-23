#!/usr/bin/env bash

echo "******************************************************************************"
echo "************* Custom Post Build Script -- Android Edition ********************"
echo "******************************************************************************"

echo "********************** Distribute Release Android ****************************"

aab_file=$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka3.PracticeLooper.Mobile.Android/bin/Release/de.emka3.practice_looper-Signed.aab

appcenter apps list --token $appcenter_token_simon
echo appcenter apps list --token $appcenter_token_simon
appcenter distribute release --app emka3/PracticeLooper --file $aab_file --group "Collaborators" --token $appcenter_token_simon