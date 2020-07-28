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
successfull=0
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
			# tests were executed successfull
			echo "Unit Tests for project $(basename "${projectFile}") passed!"
			successfull=$((successfull+1))
		else
			# tests failed
			echo "Unit Tests for project $(basename "${projectFile}") failed!"
			error=$((error+1))
		fi
	fi
	echo "-----------------"
done

if [[ $successfull -eq $numberOfTests  ]]
then
	echo "All tests were executed successfull. App will build now in next step."
	echo "Build successfull!"
else
	echo "${error} of ${numberOfTests} are failed. ${successfull} of ${numberOfTests} passed."
	echo "Build failed!"
	exit 1
fi

echo "******************************************************************************"
echo "**************************** Finish Tests ************************************"
echo "******************************************************************************"
