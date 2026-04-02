using VrtAimTrainer.Core.Features.Analytics;
using VrtAimTrainer.Core.Features.BallSheetClassic;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Tests.Features.Analytics;

public sealed class SessionAnalyticsCalculatorTests
{
    [Fact]
    public void CreateResult_ComputesExpectedSummaryValues()
    {
        var calculator = new SessionAnalyticsCalculator();
        var settings = new BallSheetClassicGamemode().CreateDefaultSettings();
        var samples = new[]
        {
            new HitSample(1.0d, 0.10d, 35d, new AimPoint(100f, 100f), 80d, 100d, null, 100d, 135d, 0d, 35d),
            new HitSample(2.0d, 0.08d, 35d, new AimPoint(150f, 160f), 320d, 400d, 45d, 120d, 155d, 35d, 70d),
            new HitSample(6.1d, 0.12d, 35d, new AimPoint(190f, 180f), 340d, 380d, 120d, 110d, 145d, 70d, 105d),
        };

        var result = calculator.CreateResult(
            gamemodeId: "ballsheet-classic",
            gamemodeDisplayName: "BallSheet Classic",
            settings: settings,
            samples: samples,
            score: 105d,
            elapsedRunSeconds: 6.1d,
            settingsChangedDuringRun: false);

        Assert.Equal(3, result.Summary.Hits);
        Assert.Equal(100d, result.Summary.AverageReactionMs, 3);
        Assert.Equal(10d, result.Summary.AverageHitsPerSecond, 3);
        Assert.Equal(105d / 6.1d, result.Summary.AverageEatPerSecond, 3);
        Assert.NotNull(result.Summary.MaxEat);
        Assert.NotNull(result.Summary.MaxEatReactionMs);
        Assert.NotNull(result.Summary.MaxEatTrueReactionMs);
        Assert.Equal(14d, result.Summary.MaxEat!.Value, 3);
        Assert.Equal(2500d, result.Summary.MaxEatReactionMs!.Value, 3);
        Assert.Equal(2500d, result.Summary.MaxEatTrueReactionMs!.Value, 3);
        Assert.NotNull(result.Telemetry);
        Assert.Equal(80d, result.Telemetry!.FastestReactionMs, 3);
        Assert.Equal(100d, result.Telemetry.MedianReactionMs, 3);
        Assert.Equal(118d, result.Telemetry.P95ReactionMs, 3);
        Assert.Equal(120d, result.Telemetry.SlowestReactionMs, 3);
        Assert.Equal(Math.Sqrt(((0d * 0d) + (20d * 20d) + (20d * 20d)) / 3d), result.Telemetry.ReactionStdDevMs, 3);
        Assert.Equal((80d + 320d + 340d) / 3d, result.Telemetry.AverageSpawnDistance, 3);
        Assert.Equal((100d + 400d + 380d) / 3d, result.Telemetry.AverageCursorTravelDistance, 3);
        Assert.Equal(880d, result.Telemetry.TotalCursorTravelDistance, 3);
        Assert.Equal(((80d / 100d) + (320d / 400d) + (340d / 380d)) / 3d, result.Telemetry.AveragePathEfficiency, 3);
        Assert.Equal(0.8d, result.Telemetry.MedianPathEfficiency, 3);
        Assert.Equal(((20d) + (80d) + (40d)) / 3d, result.Telemetry.AverageOvertravelDistance, 3);
        Assert.Equal((35d + 35d + 35d) / 3d, result.Telemetry.AverageHealthGainPerHit, 3);
        Assert.Equal((35d + 35d + 35d) / 3d, result.Telemetry.AverageScoreGainPerHit, 3);
    }
}
