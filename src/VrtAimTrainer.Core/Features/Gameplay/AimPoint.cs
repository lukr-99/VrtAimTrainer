namespace VrtAimTrainer.Core.Features.Gameplay;

public readonly record struct AimPoint(float X, float Y)
{
    public AimPoint Translate(float deltaX, float deltaY)
    {
        return new AimPoint(X + deltaX, Y + deltaY);
    }

    public AimPoint Clamp(AimRectangle bounds)
    {
        return new AimPoint(
            Math.Clamp(X, bounds.Left, bounds.Right),
            Math.Clamp(Y, bounds.Top, bounds.Bottom));
    }

    public static double Distance(AimPoint left, AimPoint right)
    {
        var deltaX = left.X - right.X;
        var deltaY = left.Y - right.Y;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }
}
