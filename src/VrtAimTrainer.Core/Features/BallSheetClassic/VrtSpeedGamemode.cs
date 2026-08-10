using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.BallSheetClassic;

public sealed class VrtSpeedGamemode : IAimGamemode
{
    public string Id => "vrt-speed";

    public string DisplayName => "VRT Speed";

    public string Description => "Closer reactive spawns with a shorter fixed-time round for fast VRT cleanup.";

    public AimSessionSettings CreateDefaultSettings()
    {
        return new AimSessionSettings(
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
    }

    public IAimSessionController CreateController(int viewportWidth, int viewportHeight, AimSessionSettings settings, IRandomSource randomSource)
    {
        return new AimSessionEngine(
            Id,
            DisplayName,
            viewportWidth,
            viewportHeight,
            settings,
            new FreeSpawnTargetProvider(randomSource),
            new LogarithmicPressureModel(),
            new CheeseCorrectedScoringRule());
    }

    public IReadOnlyList<GamemodePreset> CreatePresets()
    {
        var defaults = CreateDefaultSettings();
        return
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
        ];
    }
}
