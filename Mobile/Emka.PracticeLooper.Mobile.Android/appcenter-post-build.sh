#!/usr/bin/env bash

echo "******************************************************************************"
echo "************* Custom Post Build Script -- Android Edition ********************"
echo "******************************************************************************"

echo "********************** Build secrets.json Android ****************************"

aab_file=$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka3.PracticeLooper.Mobile.Android/bin/Release/de.emka3.practice_looper-Signed.aab


appcenter distribute release --app emka3/Practice Looper --file $aab_file --group "Collaborators"