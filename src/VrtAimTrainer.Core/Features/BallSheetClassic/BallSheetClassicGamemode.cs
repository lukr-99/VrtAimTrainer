using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.BallSheetClassic;

public sealed class BallSheetClassicGamemode : IAimGamemode
{
    public string Id => "ballsheet-classic";

    public string DisplayName => "BallSheet Classic";

    public string Description => "Native C# remake of BallSheet's pressure-driven reactive aim loop.";

    public AimSessionSettings CreateDefaultSettings()
    {
        return new AimSessionSettings(
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
    }

    public IAimSessionController CreateController(int viewportWidth, int viewportHeight, AimSessionSettings settings, IRandomSource randomSource)
    {
        return new BallSheetClassicSessionController(viewportWidth, viewportHeight, settings, randomSource);
    }
}
