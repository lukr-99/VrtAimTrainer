using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.Analytics;

public sealed record SessionResult(
    Guid SessionId,
    DateTimeOffset CompletedAtUtc,
    string GamemodeId,
    string GamemodeDisplayName,
    AimSessionSettings Settings,
    SessionSummary Summary,
    IReadOnlyList<HitSample> Samples,
    SessionTelemetry? Telemetry = null);
