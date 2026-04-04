# VrtAimTrainer

![Version](https://img.shields.io/badge/version-0.5.2-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![.NET](https://img.shields.io/badge/.NET-10-purple)

Native C# desktop aim trainer with:

- a low-latency `Raylib-cs` gameplay shell
- a testable core simulation for scoring, spawning, and session lifecycle
- a plugin-style gamemode catalog with built-in and external mode discovery
- built-in training presets for every mode
- local JSON session history for improvement tracking
- richer per-hit telemetry for cursor travel, path efficiency, health, and score flow
- persisted shell state for live settings, preset selection, and display profile
- an AimLabs-style top-nav and control hub for play, modes, presets, run setup, display, and analytics
- branded app/window icons and a versioned Windows installer flow
- in-app reaction graph and recent-session trend panel

## Requirements

- **OS**: Windows (x64)
- **.NET SDK**: 10.x (`global.json` pins to `10.0.201`, `rollForward: latestPatch`)

## Current modes

All modes ship as part of the built-in `VRT Built-In Modes` plugin.

### BallSheet Classic

Pressure-driven reactive aim on a free-spawn sheet:

- touch the target to start
- pressure drains health over time
- each hit records elapsed time and reaction time
- score is cheese-corrected by the configured threshold
- max-eat analytics are computed from rolling 5-second windows
- the sheet targets a `1200px` arena and auto-fits to the active display mode
- spawns enforce a minimum distance from the last target by default

### VRT Speed

Closer reactive spawns with a 30-second fixed-time round for fast VRT cleanup — same BallSheet Classic mechanics, tuned for pace.

### GridShot modes

Fixed-time grid drills where targets appear only on cell centres. Three variants ship out of the box:

| Mode | Grid | Live targets | Default round |
|------|------|-------------|---------------|
| `3x3 Quick` | 3 × 3 | 1 | 30 s |
| `5x5 Quick` | 5 × 5 | 1 | 30 s |
| `5x5 Triple` | 5 × 5 | 3 | 45 s |

## Built-in presets

Each mode ships with a set of named presets accessible from the `Presets` page.

**BallSheet Classic**

| Preset | Focus |
|--------|-------|
| BallSheet Classic Default | Balanced baseline |
| VRT Reactive Wide | Wide-sheet reactive reps with more spread and tempo |
| Micro Corrections | Smaller targets, tighter punishment for micro-adjustments |
| Smoothness Lanes | Larger travel lanes, lower pressure for full-sheet tracking |
| Endurance Builder | Longer reactive runs with more health |

**VRT Speed**

| Preset | Focus |
|--------|-------|
| VRT Speed Default | Balanced baseline |
| Close Burst | Faster short-range spam for cleanup speed |
| Micro Rush | Smaller targets, higher pace for close-quarters corrections |
| Stable 30 | Softer punishment for repeatable 30-second reps |

**3x3 Quick**

| Preset | Focus |
|--------|-------|
| 3x3 Quick Default | Balanced baseline |
| Snap Lines | Bigger targets, shorter rounds for raw lane-switch speed |
| Control Hold | Precise confirmations with a calmer tempo |

**5x5 Quick**

| Preset | Focus |
|--------|-------|
| 5x5 Quick Default | Balanced baseline |
| Precision Ladder | Smaller balls, sharper threshold for clean precision reps |
| Tempo Builder | Larger targets with more score throughput for pace building |

**5x5 Triple**

| Preset | Focus |
|--------|-------|
| 5x5 Triple Default | Balanced baseline |
| Selective Chaos | Higher pressure triple-target work for faster target triage |
| Endurance Grid | Longer blocks with larger targets to sustain accuracy |

## Controls

| Key | Action |
|-----|--------|
| `TAB` | Cycle the control-hub pages |
| `F1` | Jump straight to Analytics |
| `R` | Reset the live controller to pregame |
| `E` | Finish the current run and show results |
| `F11` | Toggle between borderless fullscreen and the default 1080p window |
| `Esc` | Open a quit warning instead of closing instantly |

## Menus

- `Play` — surfaces local benchmarks and reset/finish controls for the active run
- `Modes` — switch between built-in and external plugin-backed gamemodes; change takes effect immediately
- `Presets` — apply a named preset for the active mode; change takes effect immediately
- `Run Setup` — tune cursor size, score per hit, pressure, starting health, round time, and cheese threshold; slider changes apply on release
- `Mouse` — tune live gameplay sensitivity, per-axis scaling, and vertical inversion without touching presets
- `Sheet` — tune sheet width, sheet height, ball size, ball spread, edge padding, and top/bottom safe padding; slider changes apply on release
- `Display` — switch between `1080p`, `1440p`, and borderless fullscreen
- `Analytics` — view the reaction trend and recent saved sessions

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

## Writing a custom gamemode plugin

Any assembly that exports a public, no-arg class implementing `IAimGamemodePlugin` (from `VrtAimTrainer.Core`) is loaded automatically from the plugins directory. Drop the compiled `.dll` (and any dependencies) into:

`%LOCALAPPDATA%\VrtAimTrainer\Plugins\<your-plugin-folder>\`

The app discovers all matching assemblies on startup and registers their gamemodes alongside the built-in ones. Duplicate gamemode IDs are skipped with a load-failure notice shown on the `Modes` page.

## Structure

- `src/VrtAimTrainer.Core` — simulation, analytics, persistence, and all gamemode logic (BallSheet Classic, VRT Speed, GridShot)
- `src/VrtAimTrainer.Desktop` — rendering loop, immediate-mode UI, plugin loader, and installer tooling
- `tests/VrtAimTrainer.Core.Tests` — scoring, spawn-rule, analytics, and GridShot session coverage
- `tools/install` — PowerShell publish, install, switch, and uninstall scripts
