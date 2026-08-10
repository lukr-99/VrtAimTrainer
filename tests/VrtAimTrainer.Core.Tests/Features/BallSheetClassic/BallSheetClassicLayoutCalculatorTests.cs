using VrtAimTrainer.Core.Features.BallSheetClassic;

namespace VrtAimTrainer.Core.Tests.Features.BallSheetClassic;

public sealed class BallSheetClassicLayoutCalculatorTests
{
    [Fact]
    public void Create_FitsRectangularSheetIntoViewport()
    {
        var settings = BuiltInGamemodes.BallSheetClassic().CreateDefaultSettings() with
        {
            SheetWidth = 1400f,
            SheetHeight = 900f,
            ArenaSize = 1400f,
        };

        var layout = new BallSheetClassicLayoutCalculator().Create(1000, 1000, settings);

        Assert.Equal(1000f, layout.OuterBounds.Width, 3);
        Assert.Equal(642.857f, layout.OuterBounds.Height, 3);
        Assert.True(layout.OuterBounds.Width > layout.OuterBounds.Height);
        Assert.True(layout.SpawnBounds.Width > 0f);
        Assert.True(layout.SpawnBounds.Height > 0f);
    }
}
