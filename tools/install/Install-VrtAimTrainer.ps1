[CmdletBinding()]
param(
    [string]$PackageRoot,
    [string]$InstallRoot,
    [string]$StartMenuRoot,
    [string]$RuntimeIdentifier = 'win-x64',
    [switch]$NoActivate,
    [switch]$CreateDesktopShortcut,
    [string]$DesktopRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $PSCommandPath
. (Join-Path $scriptRoot 'VrtAimTrainer.Install.Common.ps1')

if ([string]::IsNullOrWhiteSpace($PackageRoot)) {
    $PackageRoot = $scriptRoot
}

if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = Get-VrtAimTrainerInstallRootDefault
}

if ([string]::IsNullOrWhiteSpace($StartMenuRoot)) {
    $StartMenuRoot = Get-VrtAimTrainerStartMenuRootDefault
}

if ([string]::IsNullOrWhiteSpace($DesktopRoot)) {
    $DesktopRoot = Get-VrtAimTrainerDesktopRootDefault
}

$InstallRoot = [System.IO.Path]::GetFullPath($InstallRoot)
$StartMenuRoot = [System.IO.Path]::GetFullPath($StartMenuRoot)
$DesktopRoot = [System.IO.Path]::GetFullPath($DesktopRoot)
$resolvedPackageRoot = (Resolve-Path $PackageRoot).Path

$manifestPath = Join-Path $resolvedPackageRoot $script:VrtAimTrainerManifestName
if (-not (Test-Path $manifestPath)) {
    $repoRoot = Get-VrtAimTrainerRepoRoot -ScriptRoot $scriptRoot
    if ($resolvedPackageRoot -eq $scriptRoot) {
        $version = Get-VrtAimTrainerVersion -RepoRoot $repoRoot
        $publishedPackageRoot = Join-Path $repoRoot "artifacts\releases\$version\$RuntimeIdentifier\package"
        Write-Host "Publishing the current repo build for $RuntimeIdentifier before install..."
        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $scriptRoot 'Publish-VrtAimTrainer.ps1') -RuntimeIdentifier $RuntimeIdentifier
        if ($LASTEXITCODE -ne 0) {
            throw "Publish-VrtAimTrainer.ps1 failed with exit code $LASTEXITCODE."
        }
        $resolvedPackageRoot = $publishedPackageRoot
    }
}

$manifest = Read-VrtAimTrainerReleaseManifest -PackageRoot $resolvedPackageRoot
$version = $manifest.version
$appSourceRoot = Join-Path $resolvedPackageRoot 'app'

if (-not (Test-Path $appSourceRoot)) {
    throw "The package root '$resolvedPackageRoot' does not contain an app folder."
}

$versionsRoot = Get-VrtAimTrainerVersionsRoot -InstallRoot $InstallRoot
$versionRoot = Join-Path $versionsRoot $version

New-Item -ItemType Directory -Path $versionsRoot -Force | Out-Null
if (Test-Path $versionRoot) {
    Remove-Item $versionRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $versionRoot -Force | Out-Null
Copy-Item -Path (Join-Path $appSourceRoot '*') -Destination $versionRoot -Recurse -Force
Write-VrtAimTrainerLauncher -InstallRoot $InstallRoot
Copy-VrtAimTrainerLauncherIcon -VersionRoot $versionRoot -InstallRoot $InstallRoot

if (-not $NoActivate.IsPresent) {
    Set-VrtAimTrainerCurrentVersion -InstallRoot $InstallRoot -Version $version
}

Ensure-VrtAimTrainerStartMenuShortcut -InstallRoot $InstallRoot -StartMenuRoot $StartMenuRoot

if ($CreateDesktopShortcut.IsPresent) {
    Ensure-VrtAimTrainerDesktopShortcut -InstallRoot $InstallRoot -DesktopRoot $DesktopRoot
}

Write-Host ''
Write-Host "Installed VrtAimTrainer $version"
Write-Host "Install root: $InstallRoot"
Write-Host "App version folder: $versionRoot"
if ($NoActivate.IsPresent) {
    Write-Host 'Activation skipped.'
}
else {
    Write-Host "Activated version: $version"
}
