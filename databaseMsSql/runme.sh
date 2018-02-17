#!/bin/bash 

echo ">>> Making sure MsSQL is up"
echo ">>> Sleep for 25 seconds"
sleep 25s
eval /scriptsMs/wait-for-it.sh "$MSSQL_SERVER_NAME:$MSSQL_PORT" -t 0
echo "<<< Done checking on MsSQL"
echo "<<< Migrate the database with flyway"
echo flyway -url="jdbc:jtds:sqlserver://$MSSQL_SERVER_NAME" -schemas="$MSSQL_DATABASE_NAME" -user="$MSSQL_USERNAME" -password="$MSSQL_ROOT_PASSWORD" -locations=filesystem:/scriptsMs/sql -validateOnMigrate=false migrate
eval flyway -url="jdbc:jtds:sqlserver://$MSSQL_SERVER_NAME" -schemas="$MSSQL_DATABASE_NAME" -user="$MSSQL_USERNAME" -password="$MSSQL_ROOT_PASSWORD" -locations=filesystem:/scriptsMs/sql -validateOnMigrate=false migrate
echo "<<< Finished creating the database and tables"
