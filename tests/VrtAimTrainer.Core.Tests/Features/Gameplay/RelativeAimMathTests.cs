using VrtAimTrainer.Core.Features.Gameplay;
using Xunit;

namespace VrtAimTrainer.Core.Tests.Features.Gameplay;

public sealed class RelativeAimMathTests
{
    private static readonly AimRectangle Bounds = new(0f, 0f, 1200f, 1200f);

    [Fact]
    public void ApplyRawDelta_MovesProportionallyToRawCounts()
    {
        var start = new AimPoint(100f, 100f);

        var single = RelativeAimMath.ApplyRawDelta(start, 10, 0, 1f, 1f, Bounds);
        var doubled = RelativeAimMath.ApplyRawDelta(start, 20, 0, 1f, 1f, Bounds);

        // Twice the raw movement covers exactly twice the distance: no velocity-based acceleration.
        var singleTravel = single.X - start.X;
        var doubledTravel = doubled.X - start.X;
        Assert.Equal(2d * singleTravel, doubledTravel, precision: 4);
    }

    [Fact]
    public void ApplyRawDelta_ScalesWithSensitivity()
    {
        var start = new AimPoint(200f, 200f);

        var low = RelativeAimMath.ApplyRawDelta(start, 50, 0, 0.5f, 1f, Bounds);
        var high = RelativeAimMath.ApplyRawDelta(start, 50, 0, 1.0f, 1f, Bounds);

        Assert.Equal(2d * (low.X - start.X), high.X - start.X, precision: 4);
    }

    [Fact]
    public void ApplyRawDelta_UsesExpectedGain()
    {
        var start = new AimPoint(0f, 0f);

        var result = RelativeAimMath.ApplyRawDelta(start, 100, 0, 1f, 1f, Bounds);

        Assert.Equal(100f * RelativeAimMath.LogicalUnitsPerCountAtUnitSensitivity, result.X, precision: 4);
    }

    [Fact]
    public void ApplyRawDelta_NegativeVerticalSensitivityInvertsAxis()
    {
        var start = new AimPoint(600f, 600f);

        var normal = RelativeAimMath.ApplyRawDelta(start, 0, 40, 1f, 1f, Bounds);
        var inverted = RelativeAimMath.ApplyRawDelta(start, 0, 40, 1f, -1f, Bounds);

        Assert.True(normal.Y > start.Y, "Positive raw Y should move the cursor down when not inverted.");
        Assert.True(inverted.Y < start.Y, "Positive raw Y should move the cursor up when inverted.");
        Assert.Equal(normal.Y - start.Y, start.Y - inverted.Y, precision: 4);
    }

    [Fact]
    public void ApplyRawDelta_ClampsToBounds()
    {
        var start = new AimPoint(1190f, 10f);

        var result = RelativeAimMath.ApplyRawDelta(start, 100000, -100000, 1f, 1f, Bounds);

        Assert.Equal(Bounds.Right, result.X, precision: 4);
        Assert.Equal(Bounds.Top, result.Y, precision: 4);
    }

    [Fact]
    public void ApplyRawDelta_ZeroDeltaKeepsPosition()
    {
        var start = new AimPoint(321f, 654f);

        var result = RelativeAimMath.ApplyRawDelta(start, 0, 0, 2.5f, 2.5f, Bounds);

        Assert.Equal(start, result);
    }
}
