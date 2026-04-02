using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.Analytics;

public sealed class SessionAnalyticsCalculator
{
    private const double MaxEatRangeSeconds = 5d;

    public SessionResult CreateResult(
        string gamemodeId,
        string gamemodeDisplayName,
        AimSessionSettings settings,
        IReadOnlyList<HitSample> samples,
        double score,
        double elapsedRunSeconds,
        bool settingsChangedDuringRun)
    {
        if (samples.Count == 0)
        {
            throw new InvalidOperationException("Cannot build a session result without samples.");
        }

        var durationSeconds = Math.Max(elapsedRunSeconds, samples[^1].ElapsedSeconds);
        var averageReactionMs = samples.Average(sample => sample.ReactionMilliseconds);
        var averageHitsPerSecond = averageReactionMs <= 0d ? 0d : 1000d / averageReactionMs;
        var averageEatPerSecond = durationSeconds <= 0d ? 0d : score / durationSeconds;
        var telemetry = CreateTelemetry(samples);

        var (maxEat, maxEatReactionMs, trueMaxEatReactionMs) = CalculateMaxEat(samples, settings);

        var summary = new SessionSummary(
            DurationSeconds: durationSeconds,
            Hits: samples.Count,
            AverageReactionMs: averageReactionMs,
            AverageHitsPerSecond: averageHitsPerSecond,
            AverageEatPerSecond: averageEatPerSecond,
            Score: score,
            MaxEat: maxEat,
            MaxEatReactionMs: maxEatReactionMs,
            MaxEatTrueReactionMs: trueMaxEatReactionMs,
            SettingsChangedDuringRun: settingsChangedDuringRun);

        return new SessionResult(
            SessionId: Guid.NewGuid(),
            CompletedAtUtc: DateTimeOffset.UtcNow,
            GamemodeId: gamemodeId,
            GamemodeDisplayName: gamemodeDisplayName,
            Settings: settings,
            Summary: summary,
            Samples: samples.ToArray(),
            Telemetry: telemetry);
    }

    private static SessionTelemetry CreateTelemetry(IReadOnlyList<HitSample> samples)
    {
        var sortedReactionTimes = samples
            .Select(sample => sample.ReactionMilliseconds)
            .OrderBy(value => value)
            .ToArray();

        var averageSpawnDistance = samples.Average(sample => sample.DistanceFromPreviousTarget);
        var averageCursorTravelDistance = samples.Average(sample => sample.CursorTravelDistance);
        var totalCursorTravelDistance = samples.Sum(sample => sample.CursorTravelDistance);
        var averageOvertravelDistance = samples.Average(sample => Math.Max(0d, sample.CursorTravelDistance - sample.DistanceFromPreviousTarget));
        var averageHealthGainPerHit = samples.Average(sample => sample.HealthAfterHit - sample.HealthBeforeHit);
        var averageScoreGainPerHit = samples.Average(sample => sample.ScoreAfterHit - sample.ScoreBeforeHit);
        var pathEfficiencies = samples
            .Select(sample => sample.PathEfficiency)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .OrderBy(value => value)
            .ToArray();

        return new SessionTelemetry(
            FastestReactionMs: sortedReactionTimes[0],
            MedianReactionMs: CalculatePercentile(sortedReactionTimes, 0.5d),
            P95ReactionMs: CalculatePercentile(sortedReactionTimes, 0.95d),
            SlowestReactionMs: sortedReactionTimes[^1],
            ReactionStdDevMs: CalculateStandardDeviation(sortedReactionTimes),
            AverageSpawnDistance: averageSpawnDistance,
            AverageCursorTravelDistance: averageCursorTravelDistance,
            TotalCursorTravelDistance: totalCursorTravelDistance,
            AveragePathEfficiency: pathEfficiencies.Length == 0 ? 0d : pathEfficiencies.Average(),
            MedianPathEfficiency: pathEfficiencies.Length == 0 ? 0d : CalculatePercentile(pathEfficiencies, 0.5d),
            AverageOvertravelDistance: averageOvertravelDistance,
            AverageHealthGainPerHit: averageHealthGainPerHit,
            AverageScoreGainPerHit: averageScoreGainPerHit);
    }

    private static double CalculatePercentile(IReadOnlyList<double> sortedValues, double percentile)
    {
        if (sortedValues.Count == 1)
        {
            return sortedValues[0];
        }

        var scaledIndex = percentile * (sortedValues.Count - 1);
        var lowerIndex = (int)Math.Floor(scaledIndex);
        var upperIndex = (int)Math.Ceiling(scaledIndex);
        if (lowerIndex == upperIndex)
        {
            return sortedValues[lowerIndex];
        }

        var weight = scaledIndex - lowerIndex;
        return sortedValues[lowerIndex] + ((sortedValues[upperIndex] - sortedValues[lowerIndex]) * weight);
    }

    private static double CalculateStandardDeviation(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
        {
            return 0d;
        }

        var average = values.Average();
        var variance = values
            .Select(value => value - average)
            .Select(delta => delta * delta)
            .Average();

        return Math.Sqrt(variance);
    }

    private static (double? MaxEat, double? MaxEatReactionMs, double? TrueMaxEatReactionMs) CalculateMaxEat(
        IReadOnlyList<HitSample> samples,
        AimSessionSettings settings)
    {
        if (samples[^1].ElapsedSeconds < MaxEatRangeSeconds)
        {
            return (null, null, null);
        }

        double? bestMaxEat = null;
        double? bestMaxEatReactionMs = null;
        double? bestTrueMaxEatReactionMs = null;

        for (var startIndex = 0; startIndex < samples.Count; startIndex++)
        {
            if (samples[startIndex].ElapsedSeconds > samples[^1].ElapsedSeconds - MaxEatRangeSeconds)
            {
                break;
            }

            var weightedTotal = 0d;
            var unweightedTotal = 0d;

            for (var hitIndex = startIndex; hitIndex < samples.Count; hitIndex++)
            {
                if (samples[hitIndex].ElapsedSeconds > samples[startIndex].ElapsedSeconds + MaxEatRangeSeconds)
                {
                    break;
                }

                weightedTotal += samples[hitIndex].CheeseCorrectedScore;
                unweightedTotal += settings.ScorePerHit;
            }

            var candidate = weightedTotal / MaxEatRangeSeconds;
            if (bestMaxEat.HasValue && candidate <= bestMaxEat.Value)
            {
                continue;
            }

            bestMaxEat = candidate;
            bestMaxEatReactionMs = candidate <= 0d
                ? null
                : 1000d / (candidate / settings.ScorePerHit);

            bestTrueMaxEatReactionMs = unweightedTotal <= 0d
                ? null
                : 1000d / ((unweightedTotal / MaxEatRangeSeconds) / settings.ScorePerHit);
        }

        return (bestMaxEat, bestMaxEatReactionMs, bestTrueMaxEatReactionMs);
    }
}
