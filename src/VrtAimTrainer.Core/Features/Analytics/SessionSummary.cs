namespace VrtAimTrainer.Core.Features.Analytics;

public sealed record SessionSummary(
    double DurationSeconds,
    int Hits,
    double AverageReactionMs,
    double AverageHitsPerSecond,
    double AverageEatPerSecond,
    double Score,
    double? MaxEat,
    double? MaxEatReactionMs,
    double? MaxEatTrueReactionMs,
    bool SettingsChangedDuringRun);
