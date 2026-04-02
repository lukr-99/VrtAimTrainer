namespace VrtAimTrainer.Core.Features.Gameplay;

public readonly record struct AimPlayfieldLayout(
    AimRectangle ViewportBounds,
    AimRectangle OuterBounds,
    AimRectangle InnerBounds,
    AimRectangle SpawnBounds);
