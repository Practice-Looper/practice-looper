#!/usr/bin/env bash

echo "******************************************************************************"
echo "**************** Custom Pre Build Script -- iOS Edition **********************"
echo "******************************************************************************"

echo "Running a search for XUnit test projects:"

array=(`find $APPCENTER_SOURCE_DIRECTORY/Mobile -regex '.*Tests*.csproj'`)

for projectFile in "${array[@]}"
do :
	echo "Project:  ${projectFile}" 	
    	echo "Assembly: $(dirname "${projectFile}")"
done

echo "Running XUnit tests:"
for projectFile in "${array[@]}"
do :
	echo "Execute Test: $(basename "${projectFile}")"
	cd $(dirname "${projectFile}")
	dotnet test --logger "trx;LogFileName=xunitestresults.trx";
	
	echo "Analyse Log-File:"
	testResultPath=$(find $(dirname "${projectFile}") -name 'xunitestresults.trx')
	
	if [ -z "$testResultPath" ]
	then
		echo "Found no testresults"
		echo "Unit Tests for project $(basename "${projectFile}") failed!"
		exit 1
	else 
		echo "Found testresult"
		failures=$(grep -o 'failed="0"' $testResultPath | wc -l)
		
		if [[ $failures -eq 1 ]]
		then
			echo "Unit Tests for project $(basename "${projectFile}") passed!"
		else
			echo "Unit Tests for project $(basename "${projectFile}") failed!"
			exit 1
		fi 
	fi
	echo	
done

echo "******************************************************************************"
echo "**************************** Finish Tests ************************************"
echo "******************************************************************************"