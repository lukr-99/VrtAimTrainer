using VrtAimTrainer.Core.Features.Gameplay;
using VrtAimTrainer.Core.Features.GridShot;

namespace VrtAimTrainer.Core.Features.BallSheetClassic;

/// <summary>
/// The definitions of the modes bundled with the app, expressed purely as data over the single
/// <see cref="AimGamemode"/> type. This is the one place built-in mode identity, defaults, and presets
/// live; the built-in plugin and the tests both source them here.
/// </summary>
public static class BuiltInGamemodes
{
    public static IReadOnlyList<IAimGamemode> All() =>
    [
        BallSheetClassic(),
        VrtSpeed(),
        GridShot3x3Quick(),
        GridShot5x5Quick(),
        GridShot5x5Triple(),
    ];

    public static AimGamemode BallSheetClassic()
    {
        var defaults = new AimSessionSettings(
            CursorDiameter: 30f,
            TargetDiameter: 60f,
            ScorePerHit: 35d,
            Pressure: 60d,
            StartingHealth: 100d,
            CheeseThresholdMs: 100d,
            ArenaSize: 1200f,
            SpawnInsetRatio: 1f / 6f,
            SpawnTopPadding: 30f,
            SpawnBottomPadding: 60f,
            MinimumSpawnDistance: 300f,
            SheetWidth: 1200f,
            SheetHeight: 1200f,
            FixedDurationSeconds: 45d);

        return new AimGamemode(
            "ballsheet-classic",
            "BallSheet Classic",
            "Native C# remake of BallSheet's pressure-driven reactive aim loop.",
            defaults,
            random => new FreeSpawnTargetProvider(random),
            [
                new GamemodePreset(
                    "vrt-reactive-wide",
                    "VRT Reactive Wide",
                    "Wide-sheet reactive reps with a little more spread and tempo.",
                    defaults with
                    {
                        SheetWidth = 1450f,
                        SheetHeight = 980f,
                        ArenaSize = 1450f,
                        TargetDiameter = 58f,
                        MinimumSpawnDistance = 360f,
                        ScorePerHit = 38d,
                        Pressure = 58d,
                        CheeseThresholdMs = 110d,
                    }),
                new GamemodePreset(
                    "vrt-micro",
                    "Micro Corrections",
                    "Smaller targets and tighter punishment for cleaner micro-adjustments.",
                    defaults with
                    {
                        CursorDiameter = 26f,
                        TargetDiameter = 44f,
                        MinimumSpawnDistance = 340f,
                        Pressure = 64d,
                        CheeseThresholdMs = 90d,
                        SpawnInsetRatio = 0.14f,
                    }),
                new GamemodePreset(
                    "smoothness-lanes",
                    "Smoothness Lanes",
                    "Larger travel lanes with lower pressure for smoother full-sheet tracking.",
                    defaults with
                    {
                        SheetWidth = 1500f,
                        SheetHeight = 1040f,
                        ArenaSize = 1500f,
                        TargetDiameter = 62f,
                        MinimumSpawnDistance = 390f,
                        Pressure = 46d,
                        CheeseThresholdMs = 120d,
                    }),
                new GamemodePreset(
                    "vrt-endurance",
                    "Endurance Builder",
                    "Longer reactive runs with more health and slightly softer punishment.",
                    defaults with
                    {
                        StartingHealth = 150d,
                        Pressure = 44d,
                        TargetDiameter = 66f,
                        MinimumSpawnDistance = 280f,
                        CheeseThresholdMs = 125d,
                        FixedDurationSeconds = 60d,
                    }),
            ]);
    }

    public static AimGamemode VrtSpeed()
    {
        var defaults = new AimSessionSettings(
            CursorDiameter: 30f,
            TargetDiameter: 56f,
            ScorePerHit: 30d,
            Pressure: 70d,
            StartingHealth: 100d,
            CheeseThresholdMs: 80d,
            ArenaSize: 1200f,
            SpawnInsetRatio: 0.14f,
            SpawnTopPadding: 24f,
            SpawnBottomPadding: 48f,
            MinimumSpawnDistance: 180f,
            SheetWidth: 1200f,
            SheetHeight: 1200f,
            FixedDurationSeconds: 30d);

        return new AimGamemode(
            "vrt-speed",
            "VRT Speed",
            "Closer reactive spawns with a shorter fixed-time round for fast VRT cleanup.",
            defaults,
            random => new FreeSpawnTargetProvider(random),
            [
                new GamemodePreset(
                    "close-burst",
                    "Close Burst",
                    "Faster short-range spam for cleanup speed and tempo control.",
                    defaults with
                    {
                        TargetDiameter = 54f,
                        MinimumSpawnDistance = 150f,
                        Pressure = 74d,
                        CheeseThresholdMs = 72d,
                        FixedDurationSeconds = 25d,
                    }),
                new GamemodePreset(
                    "micro-rush",
                    "Micro Rush",
                    "Smaller targets and higher pace for close-quarters micro corrections.",
                    defaults with
                    {
                        CursorDiameter = 28f,
                        TargetDiameter = 46f,
                        MinimumSpawnDistance = 170f,
                        Pressure = 78d,
                        CheeseThresholdMs = 68d,
                        FixedDurationSeconds = 25d,
                    }),
                new GamemodePreset(
                    "stable-30",
                    "Stable 30",
                    "Slightly softer punishment with the full 30-second round for repeatable reps.",
                    defaults with
                    {
                        TargetDiameter = 58f,
                        MinimumSpawnDistance = 200f,
                        Pressure = 64d,
                        CheeseThresholdMs = 86d,
                    }),
            ]);
    }

    public static AimGamemode GridShot3x3Quick()
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

        return new AimGamemode(
            "gridshot-3x3-quick",
            "3x3 Quick",
            "Fixed-time 3x3 drill with a single live target for fast lane changes.",
            defaults,
            random => new GridTargetProvider(random),
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

    public static AimGamemode GridShot5x5Quick()
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

        return new AimGamemode(
            "gridshot-5x5-quick",
            "5x5 Quick",
            "Fixed-time 5x5 precision drill with one live target and tighter transitions.",
            defaults,
            random => new GridTargetProvider(random),
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

    public static AimGamemode GridShot5x5Triple()
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

        return new AimGamemode(
            "gridshot-5x5-triple",
            "5x5 Triple",
            "Fixed-time 5x5 VRT grid with three live targets at once for faster target selection.",
            defaults,
            random => new GridTargetProvider(random),
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
