# VrtAimTrainer

Native C# desktop aim trainer scaffolded as a BallSheet-style remake with:

- a low-latency `Raylib-cs` gameplay shell
- a testable core simulation for scoring, spawning, and session lifecycle
- a plugin-style gamemode catalog with built-in and external mode discovery
- built-in training presets with a staged workflow before each run
- local JSON session history for improvement tracking
- richer per-hit telemetry for cursor travel, path efficiency, health, and score flow
- persisted shell state for live settings, draft settings, preset selection, and display profile
- an AimLabs-style top-nav and control hub for play, modes, presets, run setup, display, and analytics
- branded app/window icons and a versioned Windows installer flow
- in-app reaction graph and recent-session trend panel

## Current modes

`BallSheet Classic` ships as the first built-in plugin-backed mode and preserves the core native behavior from BallSheetOGL:

- touch the target to start
- pressure drains health over time
- each hit records elapsed time and reaction time
- score is cheese-corrected by the configured threshold
- max-eat analytics are computed from rolling 5-second windows
- the sheet targets a `1200px` arena and auto-fits to the active display mode
- spawns enforce a minimum distance from the last target by default

## Controls

- `TAB` cycle the control-hub pages
- `F1` jump straight to analytics
- `R` reset the live controller to pregame
- `E` finish the current run and show results
- `F11` toggle between borderless fullscreen and the default 1080p window
- `Esc` open a quit warning instead of closing instantly

## Menus

- `Play` shows the live vs staged workflow, applies staged changes, and surfaces local benchmarks
- `Modes` stages built-in and external plugin-backed gamemodes without interrupting the live run
- `Presets` stages built-in mode presets before you apply them on `Play`
- `Run` edits cursor, score, pressure, health, and cheese threshold in the staged draft
- `Mouse` tunes live gameplay sensitivity, per-axis scaling, and vertical inversion without touching presets
- `Sheet` edits sheet width, sheet height, ball size, ball spread, and edge padding in the staged draft
- `Display` switches between `1080p`, `1440p`, and borderless fullscreen
- `Analytics` shows the reaction trend and recent saved sessions

## Versioning

The desktop app version is defined once in:

`.\Version.props`

The executable, publish artifacts, and installer package all read from that version.

## Run

```powershell
dotnet build .\VrtAimTrainer.slnx
dotnet run --project .\src\VrtAimTrainer.Desktop\VrtAimTrainer.Desktop.csproj
```

## Tests

```powershell
dotnet test .\VrtAimTrainer.slnx
```

## Publish And Install

Build a Windows release bundle:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\install\Publish-VrtAimTrainer.ps1
```

That produces:

- `.\artifacts\releases\<version>\win-x64\app` published app files
- `.\artifacts\releases\<version>\win-x64\package` installable folder with scripts
- `.\artifacts\releases\<version>\win-x64\VrtAimTrainer-<version>-win-x64.zip` zipped installer bundle

Install the packaged build:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\artifacts\releases\<version>\win-x64\package\Install-VrtAimTrainer.ps1
```

Or double-click:

`.\artifacts\releases\<version>\win-x64\package\Install-VrtAimTrainer.cmd`

Default install layout:

- `%LOCALAPPDATA%\Programs\VrtAimTrainer\VrtAimTrainer.cmd` stable launcher
- `%LOCALAPPDATA%\Programs\VrtAimTrainer\current-version.txt` active version pointer
- `%LOCALAPPDATA%\Programs\VrtAimTrainer\versions\<version>\...` side-by-side installed builds

Switch to any installed version without reinstalling:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\install\Switch-VrtAimTrainerVersion.ps1 -Version <version>
```

List installed versions:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\install\Switch-VrtAimTrainerVersion.ps1 -List
```

Remove a version or wipe all installs:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\install\Uninstall-VrtAimTrainer.ps1 -Version <version>
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\install\Uninstall-VrtAimTrainer.ps1 -All
```

## Data

Sessions are stored as JSON under:

`%LOCALAPPDATA%\VrtAimTrainer\Sessions`

Shell state is stored under:

`%LOCALAPPDATA%\VrtAimTrainer\State\app-state.json`

External gamemode plugins are discovered from:

`%LOCALAPPDATA%\VrtAimTrainer\Plugins`

Each saved session includes:

- final settings
- summary metrics
- per-hit samples with elapsed time, reaction time, score contribution, spawn distance, cursor travel, health flow, and score flow
- session telemetry for median and p95 reaction, total cursor travel, average spawn distance, and path efficiency

The staged workflow keeps draft changes out of the live controller until you apply them on the `Play` page, so changing presets, sheet geometry, or scoring no longer mutates an active run.

## Structure

- `src/VrtAimTrainer.Core` simulation, analytics, and persistence
- `src/VrtAimTrainer.Desktop` rendering loop and immediate-mode UI
- `tests/VrtAimTrainer.Core.Tests` scoring and spawn-rule coverage
