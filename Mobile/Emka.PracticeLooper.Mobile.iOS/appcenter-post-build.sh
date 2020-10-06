#!/usr/bin/env bash

echo "******************************************************************************"
echo "**************** Custom Post Build Script -- iOS Edition *********************"
echo "******************************************************************************"

REPO_URL=https://${GIT_ACCESS_TOKEN_SIMON}@github.com/emka3/practice-looper.git
INFO_PLIST_PATH=$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka.PracticeLooper.Mobile.iOS/Info.plist

if [ "$APPCENTER_BRANCH" == "master" ];
then
  if [ "$AGENT_JOBSTATUS" == "Succeeded" ];
  then

    VERSION_STRING=$(grep -o -A1 '<key>CFBundleVersion</key>' $INFO_PLIST_PATH | grep -o '[0-9]\{1,2\}.[0-9]\{1,2\}.[0-9]\{1,2\}')

    git add ${INFO_PLIST_PATH}

    git commit -m "[AppCenter-iOS] Bump version to ${VERSION_STRING}"
    git push ${REPO_URL} HEAD:master
   
    echo "Push tag to origin"
    git tag -a v${VERSION_STRING}-iOS-${APPCENTER_XAMARIN_CONFIGURATION} -m "iOS ${APPCENTER_XAMARIN_CONFIGURATION} ${VERSION_STRING}"
    git push ${REPO_URL} v${VERSION_STRING}-iOS-${APPCENTER_XAMARIN_CONFIGURATION}
  fi

fi
