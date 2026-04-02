namespace VrtAimTrainer.Core.Features.Gameplay;

public readonly record struct HitSample(
    double ElapsedSeconds,
    double ReactionSeconds,
    double CheeseCorrectedScore,
    AimPoint TargetPosition,
    double DistanceFromPreviousTarget,
    double CursorTravelDistance,
    double? AngleFromPreviousTargetDegrees,
    double HealthBeforeHit,
    double HealthAfterHit,
    double ScoreBeforeHit,
    double ScoreAfterHit)
{
    public double ReactionMilliseconds => ReactionSeconds * 1000d;

    public double? PathEfficiency =>
        DistanceFromPreviousTarget <= 0d || CursorTravelDistance <= 0d
            ? null
            : Math.Min(1d, DistanceFromPreviousTarget / CursorTravelDistance);
}
