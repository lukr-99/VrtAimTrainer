namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>
/// The BallSheet/VRT "cheese-corrected" scoring rule: full <c>ScorePerHit</c> is only awarded once
/// the reaction time reaches the cheese threshold, so tapping a target the instant it spawns is
/// worth proportionally less.
/// </summary>
public sealed class CheeseCorrectedScoringRule : IScoringRule
{
    public double Award(double reactionSeconds, AimSessionSettings settings)
    {
        var cheeseFactor = Math.Min(reactionSeconds, settings.CheeseThresholdSeconds) / settings.CheeseThresholdSeconds;
        return settings.ScorePerHit * cheeseFactor;
    }
}
