using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.GridShot;

public sealed class GridShotCellLayoutBuilder
{
    public IReadOnlyList<AimPoint> Build(AimPlayfieldLayout layout, AimSessionSettings settings)
    {
        if (!settings.UsesGrid)
        {
            throw new InvalidOperationException("GridShot requires a grid-based session setting.");
        }

        var positions = new List<AimPoint>(settings.GridRows * settings.GridColumns);
        for (var row = 0; row < settings.GridRows; row++)
        {
            var normalizedRow = settings.GridRows == 1 ? 0.5f : row / (settings.GridRows - 1f);
            var y = layout.SpawnBounds.Top + (layout.SpawnBounds.Height * normalizedRow);

            for (var column = 0; column < settings.GridColumns; column++)
            {
                var normalizedColumn = settings.GridColumns == 1 ? 0.5f : column / (settings.GridColumns - 1f);
                var x = layout.SpawnBounds.Left + (layout.SpawnBounds.Width * normalizedColumn);
                positions.Add(new AimPoint(x, y));
            }
        }

        return positions;
    }
}
