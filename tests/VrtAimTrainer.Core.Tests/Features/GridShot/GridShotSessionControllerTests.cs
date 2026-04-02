using VrtAimTrainer.Core.Features.BallSheetClassic;
using VrtAimTrainer.Core.Features.Gameplay;
using VrtAimTrainer.Core.Features.GridShot;
using VrtAimTrainer.Core.Tests.Features.BallSheetClassic;

namespace VrtAimTrainer.Core.Tests.Features.GridShot;

public sealed class GridShotSessionControllerTests
{
    [Fact]
    public void Reset_SeedsConfiguredNumberOfLiveTargets()
    {
        var settings = new AimSessionSettings(
            CursorDiameter: 30f,
            TargetDiameter: 48f,
            ScorePerHit: 30d,
            Pressure: 58d,
            StartingHealth: 100d,
            CheeseThresholdMs: 85d,
            ArenaSize: 1200f,
            SpawnInsetRatio: 0.12f,
            SpawnTopPadding: 24f,
            SpawnBottomPadding: 48f,
            MinimumSpawnDistance: 0f,
            SheetWidth: 1200f,
            SheetHeight: 1200f,
            FixedDurationSeconds: 30d,
            GridRows: 5,
            GridColumns: 5,
            ConcurrentTargetCount: 3);

        var controller = new GridShotSessionController(
            gamemodeId: "gridshot-5x5-triple",
            displayName: "5x5 Triple",
            viewportWidth: 1600,
            viewportHeight: 900,
            settings: settings,
            randomSource: new SequenceRandomSource(new[] { 0.0d, 0.2d, 0.4d }));

        Assert.Equal(3, controller.State.Targets.Count);
        Assert.Equal(3, controller.State.Targets.Select(target => target.Position).Distinct().Count());
    }

    [Fact]
    public void Start_ThenUpdate_ReplacesHitTargetAndKeepsSingleLiveTarget()
    {
        var settings = new AimSessionSettings(
            CursorDiameter: 30f,
            TargetDiameter: 62f,
            ScorePerHit: 32d,
            Pressure: 56d,
            StartingHealth: 100d,
            CheeseThresholdMs: 90d,
            ArenaSize: 1100f,
            SpawnInsetRatio: 0.16f,
            SpawnTopPadding: 24f,
            SpawnBottomPadding: 48f,
            MinimumSpawnDistance: 0f,
            SheetWidth: 1100f,
            SheetHeight: 1100f,
            FixedDurationSeconds: 30d,
            GridRows: 3,
            GridColumns: 3,
            ConcurrentTargetCount: 1);

        var controller = new GridShotSessionController(
            gamemodeId: "gridshot-3x3-quick",
            displayName: "3x3 Quick",
            viewportWidth: 1600,
            viewportHeight: 900,
            settings: settings,
            randomSource: new SequenceRandomSource(new[] { 0.0d, 0.5d, 0.9d }));

        var started = controller.Start(0d);

        Assert.True(started);
        var liveTarget = controller.State.Targets[0];
        controller.Update(new AimFrameInput(0.06d, liveTarget.Position));

        var sample = Assert.Single(controller.Samples);
        Assert.Equal(SessionPhase.InGame, controller.State.Phase);
        Assert.Single(controller.State.Targets);
        Assert.NotEqual(liveTarget.Position, controller.State.Targets[0].Position);
        Assert.Equal(liveTarget.Position, sample.TargetPosition);
    }
}
