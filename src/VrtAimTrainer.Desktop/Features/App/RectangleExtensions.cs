using Raylib_cs;

namespace VrtAimTrainer.Desktop.Features.App;

internal static class RectangleExtensions
{
    public static float Bottom(this Rectangle rectangle)
    {
        return rectangle.Y + rectangle.Height;
    }

    public static float Right(this Rectangle rectangle)
    {
        return rectangle.X + rectangle.Width;
    }
}
