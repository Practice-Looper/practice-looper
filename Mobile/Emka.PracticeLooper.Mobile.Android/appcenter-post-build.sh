#!/usr/bin/env bash

echo "******************************************************************************"
echo "************** Custom Pre Build Script -- Android Edition ********************"
echo "******************************************************************************"

echo "Running a search for XUnit test projects:"
find $APPCENTER_SOURCE_DIRECTORY -regex '.Tests.csproj' -exec echo {} \;
echo 

echo "Running XUnit tests:"
find $APPCENTER_SOURCE_DIRECTORY -regex '.Tests.csproj' | xargs dotnet test --logger "trx;LogFileName=xunitestresults.trx";
echo

echo "Reading XUnit tests result"
testResultPath=$(find $APPCENTER_SOURCE_DIRECTORY -name 'xunitestresults.trx')
echo

grep ' [FAIL]' $testResultPath
failures=$(grep -o ' [FAIL]' $testResultPath | wc -l)

if [[ $failures -eq 0 ]]
then
echo "Unit Tests passed"
else
echo "Unit Tests failed"
exit 1
fi 