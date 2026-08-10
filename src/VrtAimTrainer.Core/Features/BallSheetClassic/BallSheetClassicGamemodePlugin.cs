using VrtAimTrainer.Core.Features.Gameplay;
using VrtAimTrainer.Core.Features.GridShot;

namespace VrtAimTrainer.Core.Features.BallSheetClassic;

public sealed class BallSheetClassicGamemodePlugin : IAimGamemodePlugin
{
    public AimGamemodePluginMetadata Metadata { get; } = new(
        PluginId: "vrtaimtrainer.builtin.ballsheet",
        DisplayName: "VRT Built-In Modes",
        Version: "0.5.2",
        Author: "VrtAimTrainer",
        Description: "Bundled pressure-driven reactive aim modes shipped with the desktop app.");

    public IReadOnlyList<IAimGamemode> CreateGamemodes()
    {
        return
        [
            new BallSheetClassicGamemode(),
            new VrtSpeedGamemode(),
            CreateGridShot3x3Quick(),
            CreateGridShot5x5Quick(),
            CreateGridShot5x5Triple(),
        ];
    }

    private static GridShotGamemode CreateGridShot3x3Quick()
    {
        var defaults = new AimSessionSettings(
            CursorDiameter: 30f,
            TargetDiameter: 62f,
            ScorePerHit: 32d,
            Pressure: 56d,
            StartingHealth: 100d,
            CheeseThresholdMs: 90d,
            ArenaSize: 1100f,
            SpawnInsetRatio: 0.16f,
            SpawnTopPadding: 24f,
            SpawnBottomPadding: 48f,
            MinimumSpawnDistance: 0f,
            SheetWidth: 1100f,
            SheetHeight: 1100f,
            FixedDurationSeconds: 30d,
            GridRows: 3,
            GridColumns: 3,
            ConcurrentTargetCount: 1);

        return new GridShotGamemode(
            "gridshot-3x3-quick",
            "3x3 Quick",
            "Fixed-time 3x3 drill with a single live target for fast lane changes.",
            defaults,
            [
                new GamemodePreset(
                    "snap-lines",
                    "Snap Lines",
                    "Bigger targets and shorter rounds for raw lane-switch speed.",
                    defaults with { TargetDiameter = 68f, Pressure = 60d, FixedDurationSeconds = 25d }),
                new GamemodePreset(
                    "control-hold",
                    "Control Hold",
                    "More precise 3x3 confirmations with a calmer tempo and smaller balls.",
                    defaults with { TargetDiameter = 56f, Pressure = 48d, CheeseThresholdMs = 105d, FixedDurationSeconds = 35d }),
            ]);
    }

    private static GridShotGamemode CreateGridShot5x5Quick()
    {
        var defaults = new AimSessionSettings(
            CursorDiameter: 30f,
            TargetDiameter: 48f,
            ScorePerHit: 30d,
            Pressure: 58d,
            StartingHealth: 100d,
            CheeseThresholdMs: 85d,
            ArenaSize: 1200f,
            SpawnInsetRatio: 0.12f,
            SpawnTopPadding: 24f,
            SpawnBottomPadding: 48f,
            MinimumSpawnDistance: 0f,
            SheetWidth: 1200f,
            SheetHeight: 1200f,
            FixedDurationSeconds: 30d,
            GridRows: 5,
            GridColumns: 5,
            ConcurrentTargetCount: 1);

        return new GridShotGamemode(
            "gridshot-5x5-quick",
            "5x5 Quick",
            "Fixed-time 5x5 precision drill with one live target and tighter transitions.",
            defaults,
            [
                new GamemodePreset(
                    "precision-ladder",
                    "Precision Ladder",
                    "Smaller balls and a sharper threshold for clean 5x5 precision reps.",
                    defaults with { CursorDiameter = 28f, TargetDiameter = 42f, Pressure = 62d, CheeseThresholdMs = 72d }),
                new GamemodePreset(
                    "tempo-builder",
                    "Tempo Builder",
                    "Slightly larger targets with more score throughput for pace building.",
                    defaults with { TargetDiameter = 52f, ScorePerHit = 34d, Pressure = 54d, FixedDurationSeconds = 35d }),
            ]);
    }

    private static GridShotGamemode CreateGridShot5x5Triple()
    {
        var defaults = new AimSessionSettings(
            CursorDiameter: 30f,
            TargetDiameter: 48f,
            ScorePerHit: 32d,
            Pressure: 60d,
            StartingHealth: 100d,
            CheeseThresholdMs: 90d,
            ArenaSize: 1200f,
            SpawnInsetRatio: 0.12f,
            SpawnTopPadding: 24f,
            SpawnBottomPadding: 48f,
            MinimumSpawnDistance: 0f,
            SheetWidth: 1200f,
            SheetHeight: 1200f,
            FixedDurationSeconds: 45d,
            GridRows: 5,
            GridColumns: 5,
            ConcurrentTargetCount: 3);

        return new GridShotGamemode(
            "gridshot-5x5-triple",
            "5x5 Triple",
            "Fixed-time 5x5 VRT grid with three live targets at once for faster target selection.",
            defaults,
            [
                new GamemodePreset(
                    "selective-chaos",
                    "Selective Chaos",
                    "Higher pressure triple-target work for faster target triage under load.",
                    defaults with { Pressure = 66d, CheeseThresholdMs = 78d, FixedDurationSeconds = 40d }),
                new GamemodePreset(
                    "endurance-grid",
                    "Endurance Grid",
                    "Longer triple-target blocks with larger targets to sustain accuracy under fatigue.",
                    defaults with { TargetDiameter = 52f, Pressure = 52d, StartingHealth = 120d, FixedDurationSeconds = 55d }),
            ]);
    }
}
