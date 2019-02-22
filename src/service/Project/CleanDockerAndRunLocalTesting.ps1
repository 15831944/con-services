# Usage, e.g: .\CleanDockerAndRunLocalTesting.ps1 --detach --no-log

[console]::ResetColor()

# If regularly re running the script on the same service it's faster to opt out of setting the environment vars each time.
IF (-not($args -contains "--no-vars")) { & .\set-environment-variables.ps1 }

Write-Host "Stopping Docker containers"
docker ps -q | ForEach-Object { docker stop $_ }

# This is not ideal; but too often the containers fail to start due to drive or volume errors on the existing containers.
Write-Host "Removing old application containers"
docker ps -aq --filter "name=project_" | ForEach-Object { docker rm $_ }

Write-Host "Connecting to image host" -ForegroundColor DarkGray
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)

Write-Host "Building solution" -ForegroundColor DarkGray

$artifactsWorkingDir = "${PSScriptRoot}/artifacts/ProjectWebApi"

Remove-Item -Path ./artifacts -Recurse -Force -ErrorAction Ignore
Invoke-Expression "dotnet publish ./src/ProjectWebApi/VSS.Project.WebApi.csproj -o ../../artifacts/ProjectWebApi -f netcoreapp2.1 -c Docker"
Invoke-Expression "dotnet build ./test/UnitTests/MasterDataProjectTests/VSS.Project.WebApi.Tests.csproj"
Copy-Item ./src/ProjectWebApi/appsettings.json $artifactsWorkingDir
New-Item -ItemType directory ./artifacts/logs | out-null

Write-Host "Copying static deployment files" -ForegroundColor DarkGray
Set-Location ./src/ProjectWebApi
Copy-Item ./appsettings.json $artifactsWorkingDir
Copy-Item ./Dockerfile $artifactsWorkingDir
Copy-Item ./web.config $artifactsWorkingDir
Copy-Item ./log4net.xml $artifactsWorkingDir

& $PSScriptRoot/AcceptanceTests/Scripts/deploy_win.ps1

Write-Host "Building image dependencies" -ForegroundColor DarkGray
Set-Location $PSScriptRoot
Invoke-Expression "docker-compose --file docker-compose-local.yml pull"

Write-Host "Building Docker containers" -ForegroundColor DarkGray

# This legacy setting suppresses logging to the console by piping it to a file on disk. If you're looking for the application logs from within the container see .artifacts/logs/.
$logToFile = IF ($args -contains "--no-log") { "" } ELSE { "> C:\Temp\output.log" }
$detach = IF ($args -contains "--detach") { "--detach" } ELSE { "" }

Set-Location $PSScriptRoot
Invoke-Expression "docker-compose --file docker-compose-local.yml up --build $detach $logToFile"

[Console]::ResetColor()

IF (-not $?) {
    Write-Host "Error: Environment failed to start" -ForegroundColor Red
    Exit 1
}

Write-Host "Finished" -ForegroundColor Green
