namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>
/// Pure math for the projected ("game-like") aim response.
/// <para>
/// A real FPS turns raw mouse counts into a <em>constant angular</em> yaw/pitch change and then
/// renders the world through a perspective (tangent) projection. The visible consequence is that
/// the crosshair covers more screen near the edges than near the centre for the same mouse motion.
/// This class reproduces that on the flat sheet: raw counts advance an angle at a fixed
/// degrees-per-count rate, and <see cref="AnglesToPoint"/> projects the angle onto the sheet so the
/// cursor speeds up toward the edges just like an in-game camera.
/// </para>
/// </summary>
public static class ProjectedAimMath
{
    private const double InchesToCentimeters = 2.54d;
    private const double DegreesToRadians = Math.PI / 180d;
    private const double RadiansToDegrees = 180d / Math.PI;

    /// <summary>
    /// Angular movement per raw mouse count, derived from the physical sensitivity (cm per 360°)
    /// and the mouse DPI. This is the value that keeps hand-travel identical to the player's game.
    /// </summary>
    public static double DegreesPerCount(double centimetersPer360, int mouseDpi)
    {
        if (centimetersPer360 <= 0d || mouseDpi <= 0)
        {
            return 0d;
        }

        var countsPer360 = centimetersPer360 * mouseDpi / InchesToCentimeters;
        return countsPer360 <= 0d ? 0d : 360d / countsPer360;
    }

    /// <summary>Vertical FOV that a horizontal FOV yields at the given aspect ratio (square pixels).</summary>
    public static double VerticalFovDegrees(double horizontalFovDegrees, double aspectWidth, double aspectHeight)
    {
        if (aspectWidth <= 0d || aspectHeight <= 0d)
        {
            return horizontalFovDegrees;
        }

        var aspect = aspectWidth / aspectHeight;
        var horizontalHalf = horizontalFovDegrees * 0.5d * DegreesToRadians;
        var verticalHalf = Math.Atan(Math.Tan(horizontalHalf) / aspect);
        return verticalHalf * 2d * RadiansToDegrees;
    }

    /// <summary>Largest yaw magnitude (degrees) that still lands inside the sheet horizontally.</summary>
    public static double MaxYawDegrees(double horizontalFovDegrees) => horizontalFovDegrees * 0.5d;

    /// <summary>Largest pitch magnitude (degrees) that still lands inside the sheet vertically.</summary>
    public static double MaxPitchDegrees(double verticalFovDegrees) => verticalFovDegrees * 0.5d;

    /// <summary>
    /// Projects a yaw/pitch angle (degrees, relative to the sheet centre) onto a logical sheet point.
    /// The result is clamped to <paramref name="bounds"/>.
    /// </summary>
    public static AimPoint AnglesToPoint(
        double yawDegrees,
        double pitchDegrees,
        double horizontalFovDegrees,
        double verticalFovDegrees,
        AimRectangle bounds)
    {
        var center = bounds.Center;
        var focalX = HalfExtentFocal(bounds.Width * 0.5d, horizontalFovDegrees);
        var focalY = HalfExtentFocal(bounds.Height * 0.5d, verticalFovDegrees);

        var x = center.X + (focalX * Math.Tan(yawDegrees * DegreesToRadians));
        var y = center.Y + (focalY * Math.Tan(pitchDegrees * DegreesToRadians));

        return new AimPoint((float)x, (float)y).Clamp(bounds);
    }

    /// <summary>Inverse of <see cref="AnglesToPoint"/> — recovers the yaw/pitch for a sheet point.</summary>
    public static (double YawDegrees, double PitchDegrees) PointToAngles(
        AimPoint point,
        double horizontalFovDegrees,
        double verticalFovDegrees,
        AimRectangle bounds)
    {
        var center = bounds.Center;
        var focalX = HalfExtentFocal(bounds.Width * 0.5d, horizontalFovDegrees);
        var focalY = HalfExtentFocal(bounds.Height * 0.5d, verticalFovDegrees);

        var yaw = focalX <= 0d ? 0d : Math.Atan((point.X - center.X) / focalX) * RadiansToDegrees;
        var pitch = focalY <= 0d ? 0d : Math.Atan((point.Y - center.Y) / focalY) * RadiansToDegrees;
        return (yaw, pitch);
    }

    private static double HalfExtentFocal(double halfExtent, double fovDegrees)
    {
        var half = Math.Clamp(fovDegrees, 1d, 179d) * 0.5d * DegreesToRadians;
        return halfExtent / Math.Tan(half);
    }
}
