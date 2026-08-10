namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>
/// Pure relative-aim math for the gameplay cursor.
/// <para>
/// Converts an <em>unaccelerated</em> raw mouse delta (device counts, as delivered by the
/// Windows Raw Input API) into a new logical cursor position. Because it works from raw
/// device counts rather than the OS cursor position, it is completely free of Windows
/// "Enhance pointer precision" pointer ballistics — moving the mouse twice as fast covers
/// exactly twice the distance, never more.
/// </para>
/// <para>
/// It is also deliberately independent of the render/display scale: sensitivity is expressed
/// in logical playfield units per raw count, so the feel never changes when the sheet size,
/// window size, or display profile changes.
/// </para>
/// </summary>
public static class RelativeAimMath
{
    /// <summary>
    /// Logical playfield units travelled per raw mouse count at an effective sensitivity of 1.0.
    /// Kept modest so the default sensitivity feels controllable on a typical 800 DPI mouse;
    /// the per-user <c>Sensitivity</c> slider scales around it.
    /// </summary>
    public const float LogicalUnitsPerCountAtUnitSensitivity = 0.6f;

    /// <summary>
    /// Applies a raw device delta to the current logical cursor position and clamps the result
    /// to <paramref name="bounds"/>.
    /// </summary>
    /// <param name="current">Current logical cursor position.</param>
    /// <param name="rawDeltaX">Raw horizontal device counts since the last frame (right is positive).</param>
    /// <param name="rawDeltaY">Raw vertical device counts since the last frame (down is positive).</param>
    /// <param name="horizontalSensitivity">Effective horizontal sensitivity (sensitivity × horizontal scale).</param>
    /// <param name="verticalSensitivity">
    /// Effective vertical sensitivity (sensitivity × vertical scale). Pass a negative value to invert
    /// the vertical axis.
    /// </param>
    /// <param name="bounds">Logical bounds the cursor is confined to.</param>
    public static AimPoint ApplyRawDelta(
        AimPoint current,
        int rawDeltaX,
        int rawDeltaY,
        float horizontalSensitivity,
        float verticalSensitivity,
        AimRectangle bounds)
    {
        var logicalDeltaX = rawDeltaX * horizontalSensitivity * LogicalUnitsPerCountAtUnitSensitivity;
        var logicalDeltaY = rawDeltaY * verticalSensitivity * LogicalUnitsPerCountAtUnitSensitivity;

        return current
            .Translate(logicalDeltaX, logicalDeltaY)
            .Clamp(bounds);
    }
}
