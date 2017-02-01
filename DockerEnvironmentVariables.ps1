<#
   To setup environment variables to allow you to debug using your local mysql container:
   
   1) File\Open Windows PowerShell\Open Windows PowerShell as administrator
   2) change directory to the folder containing this file
   3) type (or copy) this command and run in PS: Set-ExecutionPolicy RemoteSigned
   4) type (or copy) this command and press enter in PS: .\DockerEnvironmentVariables.ps1
   Note: if you change environment settings whilst you have the Visual Studio open.
   	You need to resart Visual Studio

#>

[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME", "VSS-Productivity", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "abc123", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB", "localhost", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_ReadVSPDB", "localhost", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_URI", "localhost", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_PORT", "9092", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_GROUP_NAME", "UnifiedProductivity-Datafeed", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_TOPIC_NAME_SUFFIX", "-VUP", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://webapi:80/api/v3/productivity/", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://localhost:5000/api/v3/productivity/", "Machine")
[Environment]::SetEnvironmentVariable("UTILIZATION_URL", "AcceptanceTestUtilizationData.json", "Machine")

<#  Dev environment
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME", "VSS-Productivity", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "d3vRDS1234_", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_ReadVSPDB", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_URI", "10.97.99.172", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_PORT", "9092", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_GROUP_NAME", "UnifiedProductivity-Datafeed", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_TOPIC_NAME_SUFFIX", "-Dev", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://10.97.96.118:3001/api/v1/productivity/", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://10.97.96.118:3001/api/v1/productivity/", "Machine")
[Environment]::SetEnvironmentVariable("UTILIZATION_URL", "https://api-stg.trimble.com/t/trimble.com/vss-alpha-assetutilization/1.1", "Machine")
#>