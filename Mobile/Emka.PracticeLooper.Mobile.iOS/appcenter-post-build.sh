#!/usr/bin/env bash

echo "******************************************************************************"
echo "**************** Custom Post Build Script -- iOS Edition *********************"
echo "******************************************************************************"

INFO_PLIST_PATH=$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka.PracticeLooper.Mobile.iOS/Info.plist

if [ "$APPCENTER_BRANCH" == "master" ] && [ "$APPCENTER_XAMARIN_CONFIGURATION" == "Store" ] && [ "$AGENT_JOBSTATUS" == "Succeeded" ];
then
  VERSION_STRING=$(grep -o -A1 '<key>CFBundleVersion</key>' $INFO_PLIST_PATH | grep -o '[0-9]\{1,2\}.[0-9]\{1,2\}.[0-9]\{1,2\}')

  git add ${INFO_PLIST_PATH}
  git add ${APPCENTER_SOURCE_DIRECTORY}/coverage-reports/${APP_VERSION}

  git commit -m "bump version to ${VERSION_STRING}, add coverage-report for ${APP_VERSION}"
  git push ${GIT_REPO_ACCESS} HEAD:master

  echo "Push tag to origin"
  git tag -a v${VERSION_STRING}-iOS-${APPCENTER_XAMARIN_CONFIGURATION} -m "iOS ${APPCENTER_XAMARIN_CONFIGURATION} ${VERSION_STRING}"
  git push ${GIT_REPO_ACCESS} v${VERSION_STRING}-iOS-${APPCENTER_XAMARIN_CONFIGURATION}
fi
