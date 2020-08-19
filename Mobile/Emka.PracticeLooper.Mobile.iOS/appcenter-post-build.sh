#!/usr/bin/env bash

echo "******************************************************************************"
echo "**************** Custom Post Build Script -- iOS Edition *********************"
echo "******************************************************************************"


if [ "$APPCENTER_BRANCH" == "issue#51-git-version-bump" ];
then
  REPO_URL=https://${GIT_ACCESS_TOKEN_SIMON}@github.com/emka3/practice-looper.git
  INFO_PLIST_PATH=$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka.PracticeLooper.Mobile.iOS/Info.plist
  MANIFEST_PATH=$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka.PracticeLooper.Mobile.Android/Properties/AndroidManifest.xml

  git add ${INFO_PLIST_PATH}
  git add ${MANIFEST_PATH}

  VERSION_STRING=$(grep -o -A1 '<key>CFBundleVersion</key>' $INFO_PLIST_PATH | grep -o '[0-9]\{1,2\}.[0-9]\{1,2\}.[0-9]\{1,2\}')
  git commit -m "[AppCenter] Bump version to ${VERSION_STRING}"
  git push ${REPO_URL} issue\#51-git-version-bump
  git push ${REPO_URL}
  git push origin
  git tag -a v${VERSION_STRING} -m "Release ${VERSION_STRING}"
fi
