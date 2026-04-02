$script:VrtAimTrainerProductName = 'VrtAimTrainer'
$script:VrtAimTrainerExecutableName = 'VrtAimTrainer.exe'
$script:VrtAimTrainerManifestName = 'release.json'

function Get-VrtAimTrainerRepoRoot {
    param(
        [Parameter(Mandatory)]
        [string]$ScriptRoot
    )

    return (Resolve-Path (Join-Path $ScriptRoot '..\..')).Path
}

function Get-VrtAimTrainerVersion {
    param(
        [Parameter(Mandatory)]
        [string]$RepoRoot
    )

    $versionPropsPath = Join-Path $RepoRoot 'Version.props'
    if (-not (Test-Path $versionPropsPath)) {
        throw "Version.props was not found at '$versionPropsPath'."
    }

    [xml]$versionProps = Get-Content $versionPropsPath
    $version = $versionProps.Project.PropertyGroup.Version
    if ($version -like '$(*' -or [string]::IsNullOrWhiteSpace($version)) {
        $version = $versionProps.Project.PropertyGroup.VersionPrefix
    }

    if ([string]::IsNullOrWhiteSpace($version)) {
        throw "Version.props does not define a Version element."
    }

    return $version.Trim()
}

function Get-VrtAimTrainerProjectPath {
    param(
        [Parameter(Mandatory)]
        [string]$RepoRoot
    )

    return Join-Path $RepoRoot 'src\VrtAimTrainer.Desktop\VrtAimTrainer.Desktop.csproj'
}

function Get-VrtAimTrainerInstallRootDefault {
    return Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)) 'Programs\VrtAimTrainer'
}

function Get-VrtAimTrainerStartMenuRootDefault {
    return Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::ApplicationData)) 'Microsoft\Windows\Start Menu\Programs'
}

function Get-VrtAimTrainerDesktopRootDefault {
    return [Environment]::GetFolderPath([Environment+SpecialFolder]::DesktopDirectory)
}

function Get-VrtAimTrainerVersionsRoot {
    param(
        [Parameter(Mandatory)]
        [string]$InstallRoot
    )

    return Join-Path $InstallRoot 'versions'
}

function Get-VrtAimTrainerCurrentVersionPath {
    param(
        [Parameter(Mandatory)]
        [string]$InstallRoot
    )

    return Join-Path $InstallRoot 'current-version.txt'
}

function Get-VrtAimTrainerLauncherPath {
    param(
        [Parameter(Mandatory)]
        [string]$InstallRoot
    )

    return Join-Path $InstallRoot 'VrtAimTrainer.cmd'
}

function Get-VrtAimTrainerLauncherIconPath {
    param(
        [Parameter(Mandatory)]
        [string]$InstallRoot
    )

    return Join-Path $InstallRoot 'VrtAimTrainerIcon.ico'
}

function Get-VrtAimTrainerStartMenuShortcutPath {
    param(
        [Parameter(Mandatory)]
        [string]$StartMenuRoot
    )

    return Join-Path $StartMenuRoot 'VrtAimTrainer.lnk'
}

function Get-VrtAimTrainerDesktopShortcutPath {
    param(
        [Parameter(Mandatory)]
        [string]$DesktopRoot
    )

    return Join-Path $DesktopRoot 'VrtAimTrainer.lnk'
}

function Read-VrtAimTrainerReleaseManifest {
    param(
        [Parameter(Mandatory)]
        [string]$PackageRoot
    )

    $manifestPath = Join-Path $PackageRoot $script:VrtAimTrainerManifestName
    if (-not (Test-Path $manifestPath)) {
        throw "Release manifest was not found at '$manifestPath'."
    }

    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
    if ([string]::IsNullOrWhiteSpace($manifest.version)) {
        throw "Release manifest at '$manifestPath' does not include a version."
    }

    return $manifest
}

function Get-VrtAimTrainerInstalledVersions {
    param(
        [Parameter(Mandatory)]
        [string]$InstallRoot
    )

    $versionsRoot = Get-VrtAimTrainerVersionsRoot -InstallRoot $InstallRoot
    if (-not (Test-Path $versionsRoot)) {
        return @()
    }

    return Get-ChildItem $versionsRoot -Directory |
        Where-Object {
            try {
                $null = [version]$_.Name
                $true
            }
            catch {
                $false
            }
        } |
        Sort-Object { [version]$_.Name } -Descending |
        ForEach-Object { $_.Name }
}

function Get-VrtAimTrainerCurrentVersion {
    param(
        [Parameter(Mandatory)]
        [string]$InstallRoot
    )

    $currentVersionPath = Get-VrtAimTrainerCurrentVersionPath -InstallRoot $InstallRoot
    if (-not (Test-Path $currentVersionPath)) {
        return $null
    }

    $version = (Get-Content $currentVersionPath -Raw).Trim()
    if ([string]::IsNullOrWhiteSpace($version)) {
        return $null
    }

    return $version
}

function Set-VrtAimTrainerCurrentVersion {
    param(
        [Parameter(Mandatory)]
        [string]$InstallRoot,
        [Parameter(Mandatory)]
        [string]$Version
    )

    New-Item -ItemType Directory -Path $InstallRoot -Force | Out-Null
    Set-Content -Path (Get-VrtAimTrainerCurrentVersionPath -InstallRoot $InstallRoot) -Value $Version -Encoding Ascii
}

function Write-VrtAimTrainerLauncher {
    param(
        [Parameter(Mandatory)]
        [string]$InstallRoot
    )

    New-Item -ItemType Directory -Path $InstallRoot -Force | Out-Null

    $launcherContent = @'
@echo off
setlocal EnableExtensions DisableDelayedExpansion
set "APPROOT=%~dp0"
set "CURRENT_VERSION_FILE=%APPROOT%current-version.txt"
if not exist "%CURRENT_VERSION_FILE%" (
  echo No active VrtAimTrainer version is installed.
  exit /b 1
)

set /p CURRENT_VERSION=<"%CURRENT_VERSION_FILE%"
if "%CURRENT_VERSION%"=="" (
  echo No active VrtAimTrainer version is configured.
  exit /b 1
)

set "TARGET=%APPROOT%versions\%CURRENT_VERSION%\VrtAimTrainer.exe"
if not exist "%TARGET%" (
  echo Active version %CURRENT_VERSION% is missing from the install root.
  exit /b 1
)

start "" "%TARGET%"
exit /b 0
'@

    Set-Content -Path (Get-VrtAimTrainerLauncherPath -InstallRoot $InstallRoot) -Value $launcherContent -Encoding Ascii
}

function Copy-VrtAimTrainerLauncherIcon {
    param(
        [Parameter(Mandatory)]
        [string]$VersionRoot,
        [Parameter(Mandatory)]
        [string]$InstallRoot
    )

    $sourceIconPath = Join-Path $VersionRoot 'Assets\Branding\VrtAimTrainerIcon.ico'
    if (-not (Test-Path $sourceIconPath)) {
        throw "The installed build does not include the branding icon at '$sourceIconPath'."
    }

    Copy-Item -Path $sourceIconPath -Destination (Get-VrtAimTrainerLauncherIconPath -InstallRoot $InstallRoot) -Force
}

function New-VrtAimTrainerShortcut {
    param(
        [Parameter(Mandatory)]
        [string]$ShortcutPath,
        [Parameter(Mandatory)]
        [string]$TargetPath,
        [Parameter(Mandatory)]
        [string]$IconPath
    )

    New-Item -ItemType Directory -Path (Split-Path $ShortcutPath -Parent) -Force | Out-Null
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($ShortcutPath)
    $shortcut.TargetPath = $TargetPath
    $shortcut.WorkingDirectory = Split-Path $TargetPath -Parent
    $shortcut.IconLocation = $IconPath
    $shortcut.Save()
}

function Ensure-VrtAimTrainerStartMenuShortcut {
    param(
        [Parameter(Mandatory)]
        [string]$InstallRoot,
        [Parameter(Mandatory)]
        [string]$StartMenuRoot
    )

    New-VrtAimTrainerShortcut `
        -ShortcutPath (Get-VrtAimTrainerStartMenuShortcutPath -StartMenuRoot $StartMenuRoot) `
        -TargetPath (Get-VrtAimTrainerLauncherPath -InstallRoot $InstallRoot) `
        -IconPath (Get-VrtAimTrainerLauncherIconPath -InstallRoot $InstallRoot)
}

function Ensure-VrtAimTrainerDesktopShortcut {
    param(
        [Parameter(Mandatory)]
        [string]$InstallRoot,
        [Parameter(Mandatory)]
        [string]$DesktopRoot
    )

    New-VrtAimTrainerShortcut `
        -ShortcutPath (Get-VrtAimTrainerDesktopShortcutPath -DesktopRoot $DesktopRoot) `
        -TargetPath (Get-VrtAimTrainerLauncherPath -InstallRoot $InstallRoot) `
        -IconPath (Get-VrtAimTrainerLauncherIconPath -InstallRoot $InstallRoot)
}

function Remove-VrtAimTrainerShortcutIfPresent {
    param(
        [Parameter(Mandatory)]
        [string]$ShortcutPath
    )

    if (Test-Path $ShortcutPath) {
        Remove-Item $ShortcutPath -Force
    }
}
