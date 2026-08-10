namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>
/// Strategy for how a hit is scored from its reaction time. Extracting this lets modes (and future
/// plugins) change scoring without touching the session loop.
/// </summary>
public interface IScoringRule
{
    /// <summary>Score awarded for a hit taken <paramref name="reactionSeconds"/> after the previous hit.</summary>
    double Award(double reactionSeconds, AimSessionSettings settings);
}
