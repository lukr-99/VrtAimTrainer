using System.Numerics;
using Raylib_cs;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.App;

public readonly record struct ArenaRenderTransform(
    Rectangle Bounds,
    float Scale,
    int LogicalWidth,
    int LogicalHeight)
{
    public AimPoint ToLogicalPoint(Vector2 screenPoint)
    {
        var logicalX = (screenPoint.X - Bounds.X) / Scale;
        var logicalY = (screenPoint.Y - Bounds.Y) / Scale;
        return new AimPoint(logicalX, logicalY);
    }

    public Rectangle ToScreenRectangle(AimRectangle logicalRectangle)
    {
        return new Rectangle(
            Bounds.X + (logicalRectangle.X * Scale),
            Bounds.Y + (logicalRectangle.Y * Scale),
            logicalRectangle.Width * Scale,
            logicalRectangle.Height * Scale);
    }

    public Vector2 ToScreenPoint(AimPoint logicalPoint)
    {
        return new Vector2(
            Bounds.X + (logicalPoint.X * Scale),
            Bounds.Y + (logicalPoint.Y * Scale));
    }

    public float ToScreenLength(float logicalLength)
    {
        return logicalLength * Scale;
    }
}
