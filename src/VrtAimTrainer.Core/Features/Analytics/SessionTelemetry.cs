namespace VrtAimTrainer.Core.Features.Analytics;

public sealed record SessionTelemetry(
    double FastestReactionMs,
    double MedianReactionMs,
    double P95ReactionMs,
    double SlowestReactionMs,
    double ReactionStdDevMs,
    double AverageSpawnDistance,
    double AverageCursorTravelDistance,
    double TotalCursorTravelDistance,
    double AveragePathEfficiency,
    double MedianPathEfficiency,
    double AverageOvertravelDistance,
    double AverageHealthGainPerHit,
    double AverageScoreGainPerHit);
