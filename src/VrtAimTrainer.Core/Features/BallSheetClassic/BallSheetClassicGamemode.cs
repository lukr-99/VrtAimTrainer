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
        ];
    }
}
