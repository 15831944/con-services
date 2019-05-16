#This script sets up the envirionment variables for RaptorServices WebAPI
Write-host "SetupWebAPI.ps1 Version:1.0" 
Write-host "The user `"$env:username`" run SetupWebAPI.ps1 on machine `"$env:computername`" on $(Get-Date)"

$OKTORUN = "OK"

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
cd $dir
Write-host "Running from folder $dir"

$ASNIP = (Get-ChildItem Env:\ASNODEIP).Value
Write-host "ASNODEIP=$ASNIP"

$IONIP = (Get-ChildItem Env:\IONODEIP).Value
Write-host "IONODEIP=$IONIP"

$SHAREUNC = (Get-ChildItem Env:\SHAREUNC).Value
Write-host "SHAREUNC=$SHAREUNC"

$RAPTORUSERNAME = (Get-ChildItem Env:\RAPTORUSERNAME).Value
Write-host "RAPTORUSERNAME=$RAPTORUSERNAME"


if ($RAPTORUSERNAME -eq $null)
{ $RAPTORUSERNAME = "ad-vspengg\svcRaptor" }

if ($ASNIP -eq $null)
  { Write-host "Error! Environment variable ASNODEIP is not set"  -ForegroundColor Red; $OKTORUN = "Bad"}
else 
  { (Get-Content velociraptor.config.xml).replace('[ASNodeIP]', $ASNIP) | Set-Content velociraptor.config.xml}

if ($IONIP -eq $null)
  { Write-host "Error! Environment variable IONODEIP is not set"  -ForegroundColor Red; $OKTORUN = "Bad"}
else 
  { (Get-Content velociraptor.config.xml).replace('[IONodeIP]', $IONIP) | Set-Content velociraptor.config.xml}

  
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
Write-Host "Is powershell an administrator"
$currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
# now we need to mount a share for the design files and reports
if ($SHAREUNC -eq $null)
  { Write-host "Error! Environment variable SHAREUNC is not set"  -ForegroundColor Red; $OKTORUN = "Bad"}
else 
  { 
   & sc.exe qc lanmanworkstation
   & sc.exe config lanmanworkstation depend= "MrxSmb20/NSI"
   & sc.exe qc lanmanworkstation
   & sc.exe stop lanmanworkstation
   Write-Host "stop lanmanworkstation. Wait for 5 seconds"
   Start-Sleep -Second 5
   Write-Host "Start lanmanworkstation after waiting"
   & sc.exe stop lanmanworkstation
   
   Write-Host "Mapping Raptor ProductionData folder to Z: drive"
   Start-Sleep -Second 5
   $mappedDrivePassword = ConvertTo-SecureString "v3L0c1R^pt0R!" -AsPlainText -Force
   $mappedDriveUsername = $RAPTORUSERNAME
   $mappedDriveCredentials = New-Object System.Management.Automation.PSCredential ($mappedDriveUsername, $mappedDrivePassword)
   Start-Sleep -Second 5
   Write-Host "The user=$mappedDriveUsername"
   
   New-PSDrive -Name "Z" -PSProvider FileSystem -Root $SHAREUNC -Persist -Credential $mappedDriveCredentials
   Write-Host "Mapped Raptor ProductionData folder to Z: drive"
   Start-Sleep -Second 5
   #New-SmbGlobalMapping -RemotePath $SHAREUNC -Credential $mappedDriveCredentials -LocalPath Z: 
   
   & Z:
   $DL = (get-location).Drive.Name
   Write-host "Current Drive=$DL"
   if ($DL -eq "Z")
    {  & dir; & C:}
   else
    {Write-Host "Warning! Could not map IONode productionData to drive Z:"}
  }


if ($OKTORUN -eq "OK")
  {& .\\VSS.Productivity3D.WebApi.exe}
else
  { Write-host "Error! Not running VSS.Productivity3D.WebApi due to setup error. Check Environment variables ASNODEIP, IONODEIP and SHAREUNC are defined"  -ForegroundColor Red;}

