[CmdletBinding()]
param(
    [string]$RuntimeIdentifier = 'win-x64',
    [string]$Configuration = 'Release',
    [switch]$SelfContained = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $PSCommandPath
. (Join-Path $scriptRoot 'VrtAimTrainer.Install.Common.ps1')

$repoRoot = Get-VrtAimTrainerRepoRoot -ScriptRoot $scriptRoot
$projectPath = Get-VrtAimTrainerProjectPath -RepoRoot $repoRoot
$version = Get-VrtAimTrainerVersion -RepoRoot $repoRoot
$releaseRoot = Join-Path $repoRoot "artifacts\releases\$version\$RuntimeIdentifier"
$appOutputRoot = Join-Path $releaseRoot 'app'
$packageRoot = Join-Path $releaseRoot 'package'
$zipPath = Join-Path $releaseRoot "VrtAimTrainer-$version-$RuntimeIdentifier.zip"

if (Test-Path $releaseRoot) {
    Remove-Item $releaseRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $appOutputRoot -Force | Out-Null
New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null

$publishArguments = @(
    'publish',
    $projectPath,
    '--configuration',
    $Configuration,
    '--runtime',
    $RuntimeIdentifier,
    '--self-contained',
    $(if ($SelfContained.IsPresent) { 'true' } else { 'false' }),
    '--output',
    $appOutputRoot
)

Write-Host "Publishing VrtAimTrainer $version for $RuntimeIdentifier..."
& dotnet @publishArguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$manifest = [ordered]@{
    productName = $script:VrtAimTrainerProductName
    version = $version
    runtimeIdentifier = $RuntimeIdentifier
    executableName = $script:VrtAimTrainerExecutableName
    packageLayoutVersion = 1
    publishedAtUtc = [DateTime]::UtcNow.ToString('O')
}

$manifestPath = Join-Path $packageRoot $script:VrtAimTrainerManifestName
$manifest | ConvertTo-Json | Set-Content -Path $manifestPath -Encoding Ascii

New-Item -ItemType Directory -Path (Join-Path $packageRoot 'app') -Force | Out-Null
Copy-Item -Path (Join-Path $appOutputRoot '*') -Destination (Join-Path $packageRoot 'app') -Recurse -Force

$packageFiles = @(
    'Install-VrtAimTrainer.ps1',
    'Install-VrtAimTrainer.cmd',
    'Switch-VrtAimTrainerVersion.ps1',
    'Switch-VrtAimTrainerVersion.cmd',
    'Uninstall-VrtAimTrainer.ps1',
    'Uninstall-VrtAimTrainer.cmd',
    'VrtAimTrainer.Install.Common.ps1'
)

foreach ($packageFile in $packageFiles) {
    Copy-Item -Path (Join-Path $scriptRoot $packageFile) -Destination (Join-Path $packageRoot $packageFile) -Force
}

Compress-Archive -Path (Join-Path $packageRoot '*') -DestinationPath $zipPath -Force

Write-Host ''
Write-Host "Version: $version"
Write-Host "App output: $appOutputRoot"
Write-Host "Installer package: $packageRoot"
Write-Host "Zip bundle: $zipPath"
