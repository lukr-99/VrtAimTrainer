using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.BallSheetClassic;

public sealed class BallSheetClassicLayoutCalculator
{
    private readonly AimPlayfieldLayoutCalculator _layoutCalculator = new();

    public AimPlayfieldLayout Create(int viewportWidth, int viewportHeight, AimSessionSettings settings)
    {
        return _layoutCalculator.Create(viewportWidth, viewportHeight, settings);
    }
}
