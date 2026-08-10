using VrtAimTrainer.Core.Features.Gameplay;
using Xunit;

namespace VrtAimTrainer.Core.Tests.Features.Gameplay;

public sealed class ProjectedAimMathTests
{
    private static readonly AimRectangle Bounds = new(0f, 0f, 1200f, 1200f);

    [Fact]
    public void DegreesPerCount_MatchesCmPer360AndDpi()
    {
        // 30 cm/360 at 800 DPI => 30 * 800 / 2.54 = 9448.8 counts per 360°.
        var degreesPerCount = ProjectedAimMath.DegreesPerCount(30d, 800);

        Assert.Equal(360d / (30d * 800d / 2.54d), degreesPerCount, precision: 9);
    }

    [Fact]
    public void DegreesPerCount_IsHalvedWhenCmPer360Doubles()
    {
        var fast = ProjectedAimMath.DegreesPerCount(20d, 1600);
        var slow = ProjectedAimMath.DegreesPerCount(40d, 1600);

        Assert.Equal(fast, slow * 2d, precision: 9);
    }

    [Fact]
    public void VerticalFovDegrees_MatchesKnownFourThreeCase()
    {
        // 90° hFOV at 4:3 => 2 * atan(tan(45°) / (4/3)) = 2 * atan(0.75) ≈ 73.74°.
        var verticalFov = ProjectedAimMath.VerticalFovDegrees(90d, 4d, 3d);

        Assert.Equal(73.7397952d, verticalFov, precision: 5);
    }

    [Fact]
    public void VerticalFovDegrees_WiderAspectGivesSmallerVerticalFov()
    {
        var wide = ProjectedAimMath.VerticalFovDegrees(103d, 16d, 9d);
        var square = ProjectedAimMath.VerticalFovDegrees(103d, 1d, 1d);

        Assert.True(wide < square, "A wider aspect ratio should compress the vertical FOV.");
    }

    [Fact]
    public void AnglesToPoint_ZeroAngleIsSheetCentre()
    {
        var point = ProjectedAimMath.AnglesToPoint(0d, 0d, 103d, 60d, Bounds);

        Assert.Equal(Bounds.Center.X, point.X, precision: 3);
        Assert.Equal(Bounds.Center.Y, point.Y, precision: 3);
    }

    [Fact]
    public void AnglesToPoint_MaxYawReachesHorizontalEdge()
    {
        const double horizontalFov = 103d;
        var maxYaw = ProjectedAimMath.MaxYawDegrees(horizontalFov);

        var point = ProjectedAimMath.AnglesToPoint(maxYaw, 0d, horizontalFov, 60d, Bounds);

        Assert.Equal(Bounds.Right, point.X, precision: 2);
    }

    [Fact]
    public void PointToAngles_RoundTripsThroughAnglesToPoint()
    {
        const double horizontalFov = 103d;
        var verticalFov = ProjectedAimMath.VerticalFovDegrees(horizontalFov, 16d, 9d);
        var original = new AimPoint(820f, 470f);

        var (yaw, pitch) = ProjectedAimMath.PointToAngles(original, horizontalFov, verticalFov, Bounds);
        var roundTripped = ProjectedAimMath.AnglesToPoint(yaw, pitch, horizontalFov, verticalFov, Bounds);

        Assert.Equal(original.X, roundTripped.X, precision: 2);
        Assert.Equal(original.Y, roundTripped.Y, precision: 2);
    }

    [Fact]
    public void AnglesToPoint_AcceleratesTowardTheEdge()
    {
        const double horizontalFov = 103d;
        const double step = 5d;

        // The same 5° yaw step covers more screen near the edge than near the centre:
        // this is exactly the perspective "speed-up" a linear mapping lacks.
        var centreStart = ProjectedAimMath.AnglesToPoint(0d, 0d, horizontalFov, 60d, Bounds);
        var centreEnd = ProjectedAimMath.AnglesToPoint(step, 0d, horizontalFov, 60d, Bounds);
        var edgeStart = ProjectedAimMath.AnglesToPoint(40d, 0d, horizontalFov, 60d, Bounds);
        var edgeEnd = ProjectedAimMath.AnglesToPoint(40d + step, 0d, horizontalFov, 60d, Bounds);

        var centreTravel = centreEnd.X - centreStart.X;
        var edgeTravel = edgeEnd.X - edgeStart.X;

        Assert.True(edgeTravel > centreTravel,
            $"Edge travel ({edgeTravel}) should exceed centre travel ({centreTravel}).");
    }
}
