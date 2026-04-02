using VrtAimTrainer.Core.Features.BallSheetClassic;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Tests.Features.BallSheetClassic;

public sealed class BallSheetClassicTargetSpawnerTests
{
    [Fact]
    public void SpawnNextTarget_RespectsBoundsAndDistanceRule()
    {
        var settings = new BallSheetClassicGamemode().CreateDefaultSettings();
        var layout = new BallSheetClassicLayoutCalculator().Create(1600, 900, settings);
        var spawner = new BallSheetClassicTargetSpawner();
        var previousTarget = new AimPoint(layout.SpawnBounds.Left, layout.SpawnBounds.Top);

        var result = spawner.SpawnNextTarget(
            previousTarget,
            layout,
            settings,
            new SequenceRandomSource(new[] { 1d, 1d }));

        Assert.True(layout.SpawnBounds.Contains(result.Target.Position));
        Assert.True(result.DistanceFromPreviousTarget >= settings.MinimumSpawnDistance);
    }
}
