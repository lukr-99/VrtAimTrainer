using Raylib_cs;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed class VerticalFlowCursor
{
    public VerticalFlowCursor(Rectangle bounds)
    {
        X = bounds.X;
        CurrentY = bounds.Y;
        Width = bounds.Width;
    }

    public float CurrentY { get; private set; }

    public float Width { get; }

    public float X { get; }

    public Rectangle Take(float height)
    {
        var bounds = new Rectangle(X, CurrentY, Width, height);
        CurrentY = bounds.Bottom();
        return bounds;
    }

    public (Rectangle Left, Rectangle Right) TakePair(float height, float gap)
    {
        var width = (Width - gap) / 2f;
        var left = new Rectangle(X, CurrentY, width, height);
        var right = new Rectangle(X + width + gap, CurrentY, width, height);
        CurrentY = left.Bottom();
        return (left, right);
    }

    public void Space(float height)
    {
        CurrentY += height;
    }
}
