namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>Shared geometry helpers for gameplay/target math.</summary>
public static class AimGeometry
{
    /// <summary>
    /// Bearing (0–360°, math convention via atan2) from one point to another, or <c>null</c> when the
    /// two points coincide. Used to record the direction between consecutive targets for analytics.
    /// </summary>
    public static double? AngleDegreesBetween(AimPoint from, AimPoint to)
    {
        var deltaX = to.X - from.X;
        var deltaY = to.Y - from.Y;
        if (Math.Abs(deltaX) < float.Epsilon && Math.Abs(deltaY) < float.Epsilon)
        {
            return null;
        }

        var angleDegrees = Math.Atan2(deltaY, deltaX) * (180d / Math.PI);
        return angleDegrees < 0d ? angleDegrees + 360d : angleDegrees;
    }
}
