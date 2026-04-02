namespace VrtAimTrainer.Core.Features.Gameplay;

public sealed record AimSessionSettings(
    float CursorDiameter,
    float TargetDiameter,
    double ScorePerHit,
    double Pressure,
    double StartingHealth,
    double CheeseThresholdMs,
    float ArenaSize,
    float SpawnInsetRatio,
    float SpawnTopPadding,
    float SpawnBottomPadding,
    float MinimumSpawnDistance,
    float? SheetWidth = null,
    float? SheetHeight = null,
    double? FixedDurationSeconds = null,
    int GridRows = 0,
    int GridColumns = 0,
    int ConcurrentTargetCount = 1)
{
    public double CheeseThresholdSeconds => CheeseThresholdMs / 1000d;

    public float EffectiveSheetWidth => SheetWidth ?? ArenaSize;

    public float EffectiveSheetHeight => SheetHeight ?? ArenaSize;

    public bool UsesFixedDuration => FixedDurationSeconds is > 0d;

    public bool UsesGrid => GridRows > 0 && GridColumns > 0;

    public int EffectiveConcurrentTargetCount => UsesGrid
        ? Math.Clamp(ConcurrentTargetCount, 1, GridRows * GridColumns)
        : 1;

    public AimSessionSettings Normalize()
    {
        return this with
        {
            CursorDiameter = Math.Max(1f, CursorDiameter),
            TargetDiameter = Math.Max(1f, TargetDiameter),
            ScorePerHit = Math.Max(0.01d, ScorePerHit),
            Pressure = Math.Max(0.01d, Pressure),
            StartingHealth = Math.Max(1d, StartingHealth),
            CheeseThresholdMs = Math.Max(1d, CheeseThresholdMs),
            ArenaSize = Math.Max(200f, ArenaSize),
            SpawnInsetRatio = Math.Clamp(SpawnInsetRatio, 0.05f, 0.45f),
            SpawnTopPadding = Math.Max(0f, SpawnTopPadding),
            SpawnBottomPadding = Math.Max(0f, SpawnBottomPadding),
            MinimumSpawnDistance = Math.Max(0f, MinimumSpawnDistance),
            SheetWidth = Math.Max(200f, SheetWidth ?? ArenaSize),
            SheetHeight = Math.Max(200f, SheetHeight ?? ArenaSize),
            FixedDurationSeconds = FixedDurationSeconds is null
                ? null
                : Math.Max(5d, FixedDurationSeconds.Value),
            GridRows = Math.Max(0, GridRows),
            GridColumns = Math.Max(0, GridColumns),
            ConcurrentTargetCount = Math.Max(1, ConcurrentTargetCount),
        };
    }
}
