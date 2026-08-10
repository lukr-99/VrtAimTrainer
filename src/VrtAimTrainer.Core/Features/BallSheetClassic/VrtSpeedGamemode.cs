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
}
