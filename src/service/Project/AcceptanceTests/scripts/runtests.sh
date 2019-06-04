#!/bin/bash
echo "AcceptanceTests are starting .... "
echo "Wait for 30 seconds"
sleep 30s
echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
/bin/bash wait-for-it.sh db:3306 -t 0
/bin/bash wait-for-it.sh kafka:9092 -t 0
echo "Wait for 5 seconds"
sleep 5s

echo "ExecutorTests starting...."
dotnet vstest ExecutorTests/ExecutorTests.dll --logger:xunit
cp TestResults/*.trx testresults/ExecutorTests.trx
rm TestResults/*.trx
echo "ExecutorTests finished."

echo "WebApiTests starting...."
dotnet vstest WebApiTests/WebApiTests.dll --logger:xunit
cp TestResults/*.trx testresults/WebApiTests.trx
rm TestResults/*.trx
echo "WebApiTests finished."

echo "IntegrationTests starting...."
dotnet vstest IntegrationTests/IntegrationTests.dll --logger:xunit
cp TestResults/*.trx testresults/IntegrationTests.trx
rm TestResults/*.trx
echo "IntegrationTests finished."

echo " "
echo " All acceptance tests completed"
echo " "

