#!/usr/bin/env bash

echo "******************************************************************************"
echo "************** Custom Pre Build Script -- Android Edition ********************"
echo "******************************************************************************"

echo "Running a search for XUnit test projects:"

array=(`find $APPCENTER_SOURCE_DIRECTORY -regex '.*Tests*.csproj'`)

for projectFile in "${array[@]}"
do :
	echo "**************************************"
	echo "Project:  ${projectFile}" 	
    	echo "Assembly: $(dirname "${projectFile}")"
	echo "**************************************"
done

echo "Running XUnit tests:"
for projectFile in "${array[@]}"
do :
	echo "Execute Test: $(basename "${projectFile}")"
	cd $(dirname "${projectFile}")
	dotnet test --logger "trx;LogFileName=xunitestresults.trx";
	
	
	echo "Analyse Log-File:"
	testResultPath=$(find $(dirname "${projectFile}") -name 'xunitestresults.trx')
	echo $testResultPath
	if [[ $testResultPath -eq "" ]]
	then
		echo "Found no testresults"
		echo "Unit Tests for project $(basename "${projectFile}") failed!"
		exit 1
	else 
		echo "Found testresult"
		grep ' [FAIL]' $testResultPath
		failures=$(grep -o ' [FAIL]' $testResultPath | wc -l)

		if [[ $failures -eq 0 ]]
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