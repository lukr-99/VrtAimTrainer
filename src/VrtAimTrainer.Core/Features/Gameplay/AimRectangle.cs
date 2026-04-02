namespace VrtAimTrainer.Core.Features.Gameplay;

public readonly record struct AimRectangle(float X, float Y, float Width, float Height)
{
    public float Left => X;

    public float Right => X + Width;

    public float Top => Y;

    public float Bottom => Y + Height;

    public AimPoint Center => new(X + (Width / 2f), Y + (Height / 2f));

    public bool Contains(AimPoint point)
    {
        return point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
    }
}
