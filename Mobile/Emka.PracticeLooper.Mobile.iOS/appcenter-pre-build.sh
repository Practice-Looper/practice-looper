#!/usr/bin/env bash

echo "******************************************************************************"
echo "**************** Custom Pre Build Script -- iOS Edition **********************"
echo "******************************************************************************"
echo "-----------------"
echo "Running a search for XUnit test projects:"
echo "-----------------"

# only for local decelopment
# APPCENTER_SOURCE_DIRECTORY=/Users/simonsymhoven/Desktop/projects/practice-looper

array=(`find $APPCENTER_SOURCE_DIRECTORY/Mobile -regex '.*Tests*.csproj'`)
successful=0
error=0
numberOfTests=${#array[@]}

for projectFile in "${array[@]}"
do :
	echo "Project:  ${projectFile}"
	echo "Assembly: $(dirname "${projectFile}")"
	echo "-----------------"
done

echo "Running XUnit tests:"
echo "-----------------"
for projectFile in "${array[@]}"
do :
	echo "Execute Test: $(basename "${projectFile}")"

	# Change directory to current assembly
	echo "Change directory to $(dirname "${projectFile}")"
	cd $(dirname "${projectFile}")

	# Deletes old testreults if they exist
	oldTestResult=$(dirname "${projectFile}")/TestResults/xunitestresults.trx
	if [ -f $oldTestResult ] ;
	then
		echo "Delete old testResult file"
    rm $oldTestResult
	fi

	# Execute tests and save result into trx-file
	echo "Start testing .."
	dotnet test --logger "trx;LogFileName=xunitestresults.trx";

	# Execute tests and save result into trx-file
	testResultPath=$(find $(dirname "${projectFile}") -name 'xunitestresults.trx')

	# return true if testResultPath is not set or empty
	if [ -z "$testResultPath" ]
	then
		# if true, test were not executed
		echo "Found no testresults"
		echo "Unit Tests for project $(basename "${projectFile}") failed!"
		error=$((error+1))
	else
		# tests were executed
		echo "Found testresult"
		# search for failed="0" in generated trx-file and count the number
		failures=$(grep -o 'failed="0"' $testResultPath | wc -l)

		# return true if failed="0" is exactly 1
		if [[ $failures -eq 1 ]]
		then
			# tests were executed successful
			echo "Unit Tests for project $(basename "${projectFile}") passed!"
			successful=$((successful+1))
		else
			# tests failed
			echo "Unit Tests for project $(basename "${projectFile}") failed!"
			error=$((error+1))
		fi
	fi
	echo "-----------------"
done

if [[ $successful -eq $numberOfTests  ]]
then
	echo "All tests were executed successful. App will build now in next step."
	echo "Build successful!"
else
	echo "${error} of ${numberOfTests} are failed. ${successful} of ${numberOfTests} passed."
	echo "Build failed!"
	exit 1
fi

echo "******************************************************************************"
echo "**************************** Finish Tests ************************************"
echo "******************************************************************************"

echo "-----------------"
echo "Increment Build Number"
echo "-----------------"

# Path to Info.plist
INFO_PLIST_PATH="$APPCENTER_SOURCE_DIRECTORY/Mobile/Emka.PracticeLooper.Mobile.iOS/Info.plist"

# Extract version number out of Info.plist
VERSION_STRING=$(grep -o -A1 '<key>CFBundleVersion</key>' $INFO_PLIST_PATH | grep -o '[0-9]\{1,2\}.[0-9]\{1,2\}.[0-9]\{1,2\}')

echo "Actual Version Number of Repo: $VERSION_STRING"
echo "Actual Version Number of AppCenter: $APP_VERSION"

if [ $APP_VERSION != $VERSION_STRING ];
then
	# Split Version Number into different parts (Major.Minor.Build)
	IFS='.' read -r -a VERSION_ELEMENTS <<< "$VERSION_STRING"

	# Increment build number
	NEW_BUILD_NUMBER=$((VERSION_ELEMENTS[2]+1))

	# Generate new version number
	NEW_VERSION="${VERSION_ELEMENTS[0]}.${VERSION_ELEMENTS[1]}.${NEW_BUILD_NUMBER}"

	echo "New Version Number: ${NEW_VERSION}"

	# Replace bundle version in Info.plist
	plutil -replace CFBundleVersion -string "$NEW_VERSION" $INFO_PLIST_PATH
	# Replace bundle version string  in Info.plist
	plutil -replace CFBundleShortVersionString -string "$NEW_VERSION" $INFO_PLIST_PATH
fi
