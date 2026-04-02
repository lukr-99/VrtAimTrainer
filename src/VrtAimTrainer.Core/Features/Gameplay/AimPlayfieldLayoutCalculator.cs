namespace VrtAimTrainer.Core.Features.Gameplay;

public sealed class AimPlayfieldLayoutCalculator
{
    public AimPlayfieldLayout Create(int viewportWidth, int viewportHeight, AimSessionSettings settings)
    {
        var normalizedSettings = settings.Normalize();
        var scale = MathF.Min(
            viewportWidth / normalizedSettings.EffectiveSheetWidth,
            viewportHeight / normalizedSettings.EffectiveSheetHeight);
        var fittedArenaWidth = normalizedSettings.EffectiveSheetWidth * scale;
        var fittedArenaHeight = normalizedSettings.EffectiveSheetHeight * scale;
        var viewport = new AimRectangle(0f, 0f, viewportWidth, viewportHeight);

        var outerX = (viewportWidth - fittedArenaWidth) / 2f;
        var outerY = (viewportHeight - fittedArenaHeight) / 2f;
        var outerBounds = new AimRectangle(outerX, outerY, fittedArenaWidth, fittedArenaHeight);

        var innerInsetX = fittedArenaWidth * normalizedSettings.SpawnInsetRatio;
        var innerInsetY = fittedArenaHeight * normalizedSettings.SpawnInsetRatio;
        var innerBounds = new AimRectangle(
            outerBounds.X + innerInsetX,
            outerBounds.Y + innerInsetY,
            fittedArenaWidth - (innerInsetX * 2f),
            fittedArenaHeight - (innerInsetY * 2f));

        var spawnMinX = innerBounds.X + normalizedSettings.TargetDiameter;
        var spawnMinY = innerBounds.Y + normalizedSettings.TargetDiameter + normalizedSettings.SpawnTopPadding;
        var spawnWidth = Math.Max(
            1f,
            innerBounds.Width - (normalizedSettings.TargetDiameter * 2f));
        var spawnHeight = Math.Max(
            1f,
            innerBounds.Height - (normalizedSettings.TargetDiameter * 2f) - normalizedSettings.SpawnTopPadding - normalizedSettings.SpawnBottomPadding);

        var spawnBounds = new AimRectangle(spawnMinX, spawnMinY, spawnWidth, spawnHeight);
        return new AimPlayfieldLayout(viewport, outerBounds, innerBounds, spawnBounds);
    }
}
