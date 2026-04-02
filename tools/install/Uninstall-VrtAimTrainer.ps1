[CmdletBinding()]
param(
    [string]$Version,
    [string]$InstallRoot,
    [string]$StartMenuRoot,
    [string]$DesktopRoot,
    [switch]$All
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $PSCommandPath
. (Join-Path $scriptRoot 'VrtAimTrainer.Install.Common.ps1')

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
if ($All.IsPresent) {
    if (Test-Path $InstallRoot) {
        Remove-Item $InstallRoot -Recurse -Force
    }

    Remove-VrtAimTrainerShortcutIfPresent -ShortcutPath (Get-VrtAimTrainerStartMenuShortcutPath -StartMenuRoot $StartMenuRoot)
    Remove-VrtAimTrainerShortcutIfPresent -ShortcutPath (Get-VrtAimTrainerDesktopShortcutPath -DesktopRoot $DesktopRoot)
    Write-Host "Removed all VrtAimTrainer installs from '$InstallRoot'."
    return
}

if (-not $PSBoundParameters.ContainsKey('Version')) {
    throw 'Specify -Version x.y.z or pass -All.'
}

$versionRoot = Join-Path (Get-VrtAimTrainerVersionsRoot -InstallRoot $InstallRoot) $Version
if (-not (Test-Path $versionRoot)) {
    throw "Version '$Version' is not installed under '$InstallRoot'."
}

Remove-Item $versionRoot -Recurse -Force

$remainingVersions = @(Get-VrtAimTrainerInstalledVersions -InstallRoot $InstallRoot)
$currentVersion = Get-VrtAimTrainerCurrentVersion -InstallRoot $InstallRoot
if ($currentVersion -eq $Version) {
    if ($remainingVersions.Count -gt 0) {
        $fallbackVersion = $remainingVersions[0]
        Set-VrtAimTrainerCurrentVersion -InstallRoot $InstallRoot -Version $fallbackVersion
        Copy-VrtAimTrainerLauncherIcon `
            -VersionRoot (Join-Path (Get-VrtAimTrainerVersionsRoot -InstallRoot $InstallRoot) $fallbackVersion) `
            -InstallRoot $InstallRoot
        Write-Host "Removed $Version and activated fallback version $fallbackVersion."
    }
    else {
        Remove-Item (Get-VrtAimTrainerCurrentVersionPath -InstallRoot $InstallRoot) -Force -ErrorAction SilentlyContinue
        Remove-Item (Get-VrtAimTrainerLauncherPath -InstallRoot $InstallRoot) -Force -ErrorAction SilentlyContinue
        Remove-Item (Get-VrtAimTrainerLauncherIconPath -InstallRoot $InstallRoot) -Force -ErrorAction SilentlyContinue
        Remove-VrtAimTrainerShortcutIfPresent -ShortcutPath (Get-VrtAimTrainerStartMenuShortcutPath -StartMenuRoot $StartMenuRoot)
        Remove-VrtAimTrainerShortcutIfPresent -ShortcutPath (Get-VrtAimTrainerDesktopShortcutPath -DesktopRoot $DesktopRoot)
        Write-Host "Removed $Version. No installed versions remain."
    }
}
else {
    Write-Host "Removed VrtAimTrainer $Version."
}
