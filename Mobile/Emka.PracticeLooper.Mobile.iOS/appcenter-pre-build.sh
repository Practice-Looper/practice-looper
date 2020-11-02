#!/usr/bin/env bash

# only for local decelopment
# APPCENTER_SOURCE_DIRECTORY=/Users/simonsymhoven/Desktop/projects/practice-looper
# APPCENTER_SOURCE_DIRECTORY=/Volumes/Work/Projekte/Mobile/practice-looper

echo "Found NUnit test projects:"
find $APPCENTER_SOURCE_DIRECTORY -regex '.*Tests.*\.csproj' -exec echo {} \;
echo
echo "Building NUnit test projects:"
find $APPCENTER_SOURCE_DIRECTORY -regex '.*Tests.*\.csproj' -exec msbuild {} \;
echo
echo "Compiled projects to run NUnit tests:"
find $APPCENTER_SOURCE_DIRECTORY -regex '.*bin.*Tests.*\.dll' -exec echo {} \;
echo
echo "Running NUnit tests:"
find $APPCENTER_SOURCE_DIRECTORY -regex '.*bin.*Tests.*\.dll' -exec nunit3-console {} +
echo
echo "NUnit tests result:"
pathOfTestResults=$(find $APPCENTER_SOURCE_DIRECTORY -name 'TestResult.xml')
cat $pathOfTestResults
echo

#look for a failing test
grep -q 'result="Failed"' $pathOfTestResults

if [[ $? -eq 0 ]]
then
echo "A test Failed"
exit 1
else
echo "All tests passed"
fi
