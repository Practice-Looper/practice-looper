#!/usr/bin/env bash

echo "******************************************************************************"
echo "************** Custom Post Build Script -- Android Edition *******************"
echo "******************************************************************************"

REPO_URL=https://${GIT_ACCESS_TOKEN_SIMON}@github.com/emka3/practice-looper.git
MANIFEST_PATH=$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka.PracticeLooper.Mobile.Android/Properties/AndroidManifest.xml

if [ "$APPCENTER_BRANCH" == "master" ];
then
  if [ "$AGENT_JOBSTATUS" == "Succeeded" ];
  then

    VERSION_STRING=$(grep -o 'android:versionName="*.*.*" ' $MANIFEST_PATH | grep -o '[0-9]\{1,2\}.[0-9]\{1,2\}.[0-9]\{1,2\}')
    
    git add ${MANIFEST_PATH}
    git add ${APPCENTER_SOURCE_DIRECTORY}/coverage-reports/${APP_VERSION}

    git commit -m "bump version to ${VERSION_STRING}, add coverage-report for ${APP_VERSION}"
    git push ${REPO_URL} HEAD:master

    echo "Push tag to origin"
    git tag -a v${VERSION_STRING}-Android-${APPCENTER_XAMARIN_CONFIGURATION} -m "Android ${APPCENTER_XAMARIN_CONFIGURATION} ${VERSION_STRING}"
    git push ${REPO_URL} v${VERSION_STRING}-Android-${APPCENTER_XAMARIN_CONFIGURATION}
  fi
else
  if [ "$AGENT_JOBSTATUS" == "Succeeded" ];
  then
    git add ${APPCENTER_SOURCE_DIRECTORY}/coverage-reports/${APP_VERSION}
    git commit -m "add coverage-report for ${APP_VERSION}"
    git push ${REPO_URL} HEAD:${APPCENTER_BRANCH}
  fi
fi
