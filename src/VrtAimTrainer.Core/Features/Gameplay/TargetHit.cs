namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>
/// Result of a target being hit, returned by an <see cref="ITargetProvider"/>. Carries the analytics
/// the session engine records for the hit; the provider has already advanced its own target state.
/// </summary>
public readonly record struct TargetHit(
    AimPoint Position,
    double DistanceFromPreviousTarget,
    double? AngleFromPreviousTargetDegrees);
