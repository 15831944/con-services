# The purpose of this script is to centralize the setting of environment variables for local testing of the 3DP application services.

# This script assumes the following:
#
#   1. Configuration files are in YAML format.
#   2. All configuration key/value pairs start after the 'data:' line.
#   3. Comments start with #.
#   4. Key names may be mixed case.
#   5. Keys are delimetered by a colon character.
#   6. Environment variables at set at the Machine level.
#   7. It is safe to remove double quotes from values. Numbers are only escaped for ingestion into Helm.

# USAGE:
# All parameters are optional, defaults will be used if parameters are not provided, e.g.
# $ .\set-environment-variables.ps1 -useLocalOverride $false

PARAM (
    [Parameter(Mandatory = $false)][string]$globalConfigFile = "../../config/dev-config.yaml",
    [Parameter(Mandatory = $false)][string]$localConfigFile = "build/yaml/testing-configmap.yaml",
    [bool]$useLocalOverride = $true # When set will use a local projection, typically service URLs for locally running services, e.g. MockWebApi.
)

[console]::ResetColor()

$environmentVars = @{}

function SetEnvironmentVariable {
    PARAM ([string] $key, [string] $value)

    Write-Host "  " $key ": " -ForegroundColor Gray -NoNewline
    Write-Host $value -ForegroundColor DarkGray
    [Environment]::SetEnvironmentVariable($key, $value, "Machine")
}

function ProcessConfigFile {
    PARAM ([string] $filename)

    Write-Host "Reading: $filename"

    IF (-NOT $(TRY { Test-Path $filename } CATCH { $false }) ) {
        Write-Host "Error reading file '$filename': File not found." -ForegroundColor Red
        CONTINUE
    }

    [string[]]$configFile = Get-Content -Path $filename -ErrorAction Ignore

    $headerRead = $false

    FOREACH ($line in $configFile) {
        # Skip header lines.
        IF (!$headerRead -AND $line.StartsWith("data:")) { $headerRead = $true; CONTINUE }
        IF (!$headerRead) { CONTINUE }

        # Ignore comments.
        IF ($line.StartsWith("#")) { CONTINUE }

        # Ignore blank lines or trailing whitespace in the file.
        IF ([string]::IsNullOrWhiteSpace($line)) { CONTINUE }

        $key, $value = $line -split ':', 2
        $key = $key.Trim()
        $value = $value.Trim().Trim('"')

        IF ($environmentVars.ContainsKey($key)) { $environmentVars.Remove($key) }

        $environmentVars.Add($key, $value)
    }
}

# Set configuration values
ProcessConfigFile $globalConfigFile.Trim()

IF ($useLocalOverride -eq $true) { ProcessConfigFile $localConfigFile.Trim() }

Write-Host "`nSetting environment variables..."

FOREACH ($config in $environmentVars.GetEnumerator() | Sort-Object -Property Name) {
    SetEnvironmentVariable $config.Name $config.Value
}

Write-Host "`nFinished loading configuration settings" -ForegroundColor Green
