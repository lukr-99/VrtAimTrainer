namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>How raw mouse motion is mapped onto the logical playfield.</summary>
public enum AimResponseMode
{
    /// <summary>
    /// Linear 1:1 mapping — a raw count always moves the cursor the same logical distance,
    /// anywhere on the sheet. Isotropic and predictable, but unlike a real 3D FPS view.
    /// </summary>
    Linear,

    /// <summary>
    /// Projected (game-like) mapping — raw counts drive a yaw/pitch angle at a constant angular
    /// rate (as in a real FPS), and the cursor is the tangent projection of that angle. The cursor
    /// therefore accelerates toward the edges of the sheet exactly like a perspective camera, and
    /// the chosen field of view and aspect ratio govern the feel.
    /// </summary>
    Projected,
}
