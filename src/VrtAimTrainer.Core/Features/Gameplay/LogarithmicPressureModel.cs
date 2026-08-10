namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>
/// The classic BallSheet/VRT pressure curve: drain scales with configured pressure and grows
/// logarithmically as the run goes on, so a run gets steadily harder to sustain.
/// </summary>
public sealed class LogarithmicPressureModel : IPressureModel
{
    public double Apply(double currentHealth, double deltaSeconds, double elapsedRunSeconds, AimSessionSettings settings)
    {
        return currentHealth - (deltaSeconds * settings.Pressure * Math.Log(1d + elapsedRunSeconds));
    }
}
