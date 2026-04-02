namespace VrtAimTrainer.Core.Features.Gameplay;

public static class AimTargetHitTester
{
    public static bool IsCursorInsideTarget(AimPoint cursor, TargetSnapshot target, AimSessionSettings settings)
    {
        var collisionRadius = (settings.CursorDiameter + settings.TargetDiameter) / 2f;
        return AimPoint.Distance(cursor, target.Position) < collisionRadius;
    }
}
