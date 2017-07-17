
RMDIR /S /Q Artifacts
if exist Artifacts rd /s /q Artifacts

rem echo %PATH%
dotnet publish ./src/WebApi/WebApi.csproj -o ../../Artifacts/WebApi -f net47 -c Docker
dotnet build ./test/UnitTests/WebApiTests/WebApiTests.csproj
copy src\WebApi\appsettings.json Artifacts\WebApi\
copy src\WebApi\Dockerfile Artifacts\WebApi\
copy src\WebApi\SetupWebAPI.ps1 Artifacts\WebApi\
copy src\WebApi\Velociraptor.Config.Xml Artifacts\WebApi\
copy src\WebApi\web.config Artifacts\WebApi\
copy src\WebApi\log4net.xml Artifacts\WebApi\


mkdir Artifacts\Logs
dotnet build ./AcceptanceTests/tests/RaptorSvcAcceptTestsCommon/RaptorSvcAcceptTestsCommon.csproj -f net47 -c Debug
dotnet build ./AcceptanceTests/tests/ProductionDataSvc.AcceptanceTests/ProductionDataSvc.AcceptanceTests.csproj -f net47 -c Debug
rem cd .\test\ComponentTests\scripts
rem deploy_win.bat

