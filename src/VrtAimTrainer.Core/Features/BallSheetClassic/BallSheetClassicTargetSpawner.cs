using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.BallSheetClassic;

public sealed class BallSheetClassicTargetSpawner
{
    private const int MaxAttempts = 512;

    public TargetSpawnResult SpawnNextTarget(
        AimPoint previousTarget,
        AimPlayfieldLayout layout,
        AimSessionSettings settings,
        IRandomSource randomSource)
    {
        var bestCandidate = layout.SpawnBounds.Center;
        var bestDistance = double.MinValue;

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var candidate = new AimPoint(
                layout.SpawnBounds.Left + ((float)randomSource.NextDouble() * layout.SpawnBounds.Width),
                layout.SpawnBounds.Top + ((float)randomSource.NextDouble() * layout.SpawnBounds.Height));

            var distance = AimPoint.Distance(candidate, previousTarget);
            if (distance > bestDistance)
            {
                bestCandidate = candidate;
                bestDistance = distance;
            }

            if (distance >= settings.MinimumSpawnDistance)
            {
                return new TargetSpawnResult(
                    new TargetSnapshot(candidate, settings.TargetDiameter),
                    distance);
            }
        }

        return new TargetSpawnResult(
            new TargetSnapshot(bestCandidate, settings.TargetDiameter),
            Math.Max(bestDistance, 0d));
    }
}
