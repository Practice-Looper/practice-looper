#!/usr/bin/env bash

echo "******************************************************************************"
echo "**************** Custom Pre Build Script -- iOS Edition **********************"
echo "******************************************************************************"
echo "-----------------"
echo "Running a search for NUnit test projects:"
echo "-----------------"

# only for local decelopment
# APPCENTER_SOURCE_DIRECTORY=/Users/simonsymhoven/Projects/practice-looper
# APPCENTER_SOURCE_DIRECTORY=/Volumes/Work/Projekte/Mobile/practice-looper
array=(`find $APPCENTER_SOURCE_DIRECTORY/Mobile -name '*.Tests.csproj'`)
successful=0
error=0
numberOfTests=${#array[@]}

for projectFile in "${array[@]}"
do :
	echo "Project:  ${projectFile}"
	echo "Assembly: $(dirname "${projectFile}")"
	echo "-----------------"
done

echo "Running NUnit tests:"
echo "-----------------"
for projectFile in "${array[@]}"
do :
	echo "Execute Test: $(basename "${projectFile}")"

	# Change directory to current assembly
	echo "Change directory to $(dirname "${projectFile}")"
	cd $(dirname "${projectFile}")

	echo "========================================= Start testing ================================================"
	echo "================ ${projectFile} =============="
	result=$(dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput='./results/')
	echo "$result"

	if [[ $result == *"Test Run Successful."* ]];
	then
		successful=$((successful+1))
	else
		error=$((error+1))
	fi
	echo "========================================= End testing ================================================"
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
	# Replace bundle version in Info.plist
	plutil -replace CFBundleVersion -string "$APP_VERSION" $INFO_PLIST_PATH
	# Replace bundle version string  in Info.plist
	plutil -replace CFBundleShortVersionString -string "$APP_VERSION" $INFO_PLIST_PATH
fi

echo "Install ReportGenerator"
cd $APPCENTER_SOURCE_DIRECTORY/Mobile
dotnet tool install -g dotnet-reportgenerator-globaltool
dotnet tool install dotnet-reportgenerator-globaltool --tool-path tools
dotnet new tool-manifest dotnet tool install dotnet-reportgenerator-globaltool
dotnet tool restore

echo "Collect data"
DATA=""
for projectFile in "${array[@]}"
do :
	DATA+="$(dirname "${projectFile}")/results/coverage.opencover.xml;"
done
echo "$DATA"

echo "Generate report"
reportgenerator "-reports:${DATA}" "-targetdir:../coverage-reports/${APP_VERSION}"
