using VrtAimTrainer.Core.Features.BallSheetClassic;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Tests.Features.BallSheetClassic;

public sealed class BallSheetClassicSessionControllerTests
{
    [Fact]
    public void Start_BeginsRunAndScoresFirstHit()
    {
        var settings = BuiltInGamemodes.BallSheetClassic().CreateDefaultSettings();
        var controller = new AimSessionEngine(
            "ballsheet-classic",
            "BallSheet Classic",
            viewportWidth: 1600,
            viewportHeight: 900,
            settings: settings,
            targetProvider: new FreeSpawnTargetProvider(new SequenceRandomSource(new[] { 0d, 0d, 1d, 1d })),
            pressureModel: new LogarithmicPressureModel(),
            scoringRule: new CheeseCorrectedScoringRule());

        var started = controller.Start(0d);

        Assert.True(started);
        Assert.Equal(SessionPhase.InGame, controller.State.Phase);
        Assert.Empty(controller.Samples);

        var firstRunTarget = controller.State.Target.Position;
        controller.Update(new AimFrameInput(0.04d, new AimPoint(firstRunTarget.X - 120f, firstRunTarget.Y)));
        controller.Update(new AimFrameInput(0.08d, firstRunTarget));

        var sample = Assert.Single(controller.Samples);
        Assert.Equal(28d, controller.State.Score, 3);
        Assert.NotNull(controller.State.LastReactionMs);
        Assert.Equal(80d, controller.State.LastReactionMs!.Value, 3);
        Assert.Equal(1, controller.State.HitCount);
        Assert.True(controller.State.Health > settings.StartingHealth);
        Assert.True(sample.DistanceFromPreviousTarget > 0d);
        Assert.True(sample.CursorTravelDistance > 0d);
        Assert.NotNull(sample.AngleFromPreviousTargetDegrees);
        Assert.Equal(0d, sample.ScoreBeforeHit, 3);
        Assert.Equal(controller.State.Score, sample.ScoreAfterHit, 3);
    }

    [Fact]
    public void Update_FinishesFixedDurationRunWhenTimerExpires()
    {
        var settings = BuiltInGamemodes.BallSheetClassic().CreateDefaultSettings() with
        {
            FixedDurationSeconds = 5d,
        };

        var controller = new AimSessionEngine(
            "ballsheet-classic",
            "BallSheet Classic",
            viewportWidth: 1600,
            viewportHeight: 900,
            settings: settings,
            targetProvider: new FreeSpawnTargetProvider(new SequenceRandomSource(new[] { 0d, 0d, 1d, 1d })),
            pressureModel: new LogarithmicPressureModel(),
            scoringRule: new CheeseCorrectedScoringRule());

        controller.Start(0d);
        controller.Update(new AimFrameInput(0.08d, controller.State.Target.Position));
        controller.Update(new AimFrameInput(5.01d, controller.State.CursorPosition));

        Assert.Equal(SessionPhase.PostGame, controller.State.Phase);
        Assert.NotNull(controller.State.LastCompletedResult);
        Assert.Single(controller.Samples);
    }
}
