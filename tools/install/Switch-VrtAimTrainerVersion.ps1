[CmdletBinding()]
param(
    [string]$Version,
    [string]$InstallRoot,
    [switch]$List
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $PSCommandPath
. (Join-Path $scriptRoot 'VrtAimTrainer.Install.Common.ps1')

if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = Get-VrtAimTrainerInstallRootDefault
}

$InstallRoot = [System.IO.Path]::GetFullPath($InstallRoot)
$installedVersions = @(Get-VrtAimTrainerInstalledVersions -InstallRoot $InstallRoot)
if ($List.IsPresent -or -not $PSBoundParameters.ContainsKey('Version')) {
    if ($installedVersions.Count -eq 0) {
        Write-Host "No installed VrtAimTrainer versions were found under '$InstallRoot'."
        return
    }

    $currentVersion = Get-VrtAimTrainerCurrentVersion -InstallRoot $InstallRoot
    Write-Host "Installed versions under '$InstallRoot':"
    foreach ($installedVersion in $installedVersions) {
        $marker = if ($installedVersion -eq $currentVersion) { '*' } else { ' ' }
        Write-Host " $marker $installedVersion"
    }

    return
}

if ($installedVersions -notcontains $Version) {
    throw "Version '$Version' is not installed under '$InstallRoot'."
}

$versionRoot = Join-Path (Get-VrtAimTrainerVersionsRoot -InstallRoot $InstallRoot) $Version
Write-VrtAimTrainerLauncher -InstallRoot $InstallRoot
Copy-VrtAimTrainerLauncherIcon -VersionRoot $versionRoot -InstallRoot $InstallRoot
Set-VrtAimTrainerCurrentVersion -InstallRoot $InstallRoot -Version $Version

Write-Host "Activated VrtAimTrainer $Version"
Write-Host "Launcher: $(Get-VrtAimTrainerLauncherPath -InstallRoot $InstallRoot)"
