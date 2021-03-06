[Console]::ResetColor()

IF ($args -contains "--set-vars") { & ./set-environment-variables.ps1 }

Write-Host "Removing old Filter service application containers" -ForegroundColor DarkGray

# Stop and remove Filter service containers only; leave non affected containers running.
$array = @("filter_webapi", "filter_accepttest")

FOR ($i = 0; $i -lt $array.length; $i++) {
    $containerName = $array[$i]

    IF (docker ps -q --filter "name=$containerName") {
        docker stop $(docker ps -q --filter "name=$containerName")
    }

    IF (docker ps -aq --filter "name=$containerName") {
        docker rm $(docker ps -aq --filter "name=$containerName")
    }
}

Write-Host "Done" -ForegroundColor Green

Write-Host "Connecting to image host" -ForegroundColor DarkGray
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2 --profile okta)

IF (-not $?) {
    Write-Host "Error: Logging in to AWS, won't pull latest images for container dependancies." -ForegroundColor Red
}

Write-Host "Building solution" -ForegroundColor DarkGray

$artifactsWorkingDir = "${PSScriptRoot}/artifacts/VSS.Productivity3D.Filter.WebApi"

Remove-Item -Path ./artifacts -Recurse -Force -ErrorAction Ignore
Invoke-Expression "dotnet publish /nowarn:CS1591 ./src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj -o $artifactsWorkingDir -f netcoreapp3.1 -c Docker"
Invoke-Expression "dotnet build /nowarn:CS1591 ./test/UnitTests/VSS.Productivity3D.Filter.Tests/VSS.Productivity3D.Filter.Tests.csproj"
Copy-Item ./src/VSS.Productivity3D.Filter.WebApi/appsettings.json $artifactsWorkingDir
New-Item -ItemType directory ./artifacts/logs | out-null

Write-Host "Copying static deployment files" -ForegroundColor DarkGray
Set-Location ./src/VSS.Productivity3D.Filter.WebApi
Copy-Item ./appsettings.json $artifactsWorkingDir
Copy-Item ./Dockerfile $artifactsWorkingDir
Copy-Item ./web.config $artifactsWorkingDir

& $PSScriptRoot/AcceptanceTests/Scripts/deploy_win.ps1

Set-Location $PSScriptRoot
$dockerComposeConfig = "docker-compose-local"

Write-Host "Building image dependencies" -ForegroundColor DarkGray
Invoke-Expression "docker-compose --file $dockerComposeConfig.yml pull"

Write-Host "Building Docker containers" -ForegroundColor DarkGray
Invoke-Expression "docker-compose --file $dockerComposeConfig.yml up --build --detach > ${PSScriptRoot}/artifacts/logs/output.log"

IF (-not $?) {
    Write-Host "Error: Environment failed to start" -ForegroundColor Red
    EXIT 1
}

Write-Host "Finished" -ForegroundColor Green
