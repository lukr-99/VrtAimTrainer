using VrtAimTrainer.Core.Features.BallSheetClassic;
using VrtAimTrainer.Core.Features.Gameplay;
using Xunit;

namespace VrtAimTrainer.Core.Tests.Features.Gameplay;

public sealed class AimSettingsSchemaTests
{
    private static AimSessionSettings FreeSpawnSettings => new BallSheetClassicGamemode().CreateDefaultSettings();

    private static AimSessionSettings GridSettings => FreeSpawnSettings with
    {
        GridRows = 3,
        GridColumns = 3,
        ConcurrentTargetCount = 1,
    };

    [Fact]
    public void GeometryParameters_AreModeAware()
    {
        var freeKeys = AimSettingsSchema
            .ApplicableParameters(SettingGroup.Geometry, FreeSpawnSettings)
            .Select(parameter => parameter.Key)
            .ToHashSet();
        var gridKeys = AimSettingsSchema
            .ApplicableParameters(SettingGroup.Geometry, GridSettings)
            .Select(parameter => parameter.Key)
            .ToHashSet();

        // Ball spread is a free-spawn concept; grid rows/columns/live-count are grid concepts.
        Assert.Contains("spawn-safety", freeKeys);
        Assert.DoesNotContain("grid-rows", freeKeys);

        Assert.DoesNotContain("spawn-safety", gridKeys);
        Assert.Contains("grid-rows", gridKeys);
        Assert.Contains("grid-columns", gridKeys);
        Assert.Contains("grid-concurrent", gridKeys);
    }

    [Fact]
    public void EveryParameter_RoundTripsThroughSetAndGet()
    {
        var settings = GridSettings;

        foreach (var parameter in AimSettingsSchema.Parameters)
        {
            var midpoint = (parameter.Minimum + parameter.Maximum) / 2f;
            var readBack = parameter.Get(parameter.Set(settings, midpoint));

            // Within 1.0 tolerates integer-valued parameters (grid rows/columns/live count) rounding.
            Assert.True(
                Math.Abs(readBack - midpoint) <= 1.0f,
                $"Parameter '{parameter.Key}' did not round-trip: set {midpoint}, read {readBack}.");
        }
    }

    [Fact]
    public void ParameterKeys_AreUnique()
    {
        var keys = AimSettingsSchema.Parameters.Select(parameter => parameter.Key).ToArray();
        Assert.Equal(keys.Length, keys.Distinct().Count());
    }
}
