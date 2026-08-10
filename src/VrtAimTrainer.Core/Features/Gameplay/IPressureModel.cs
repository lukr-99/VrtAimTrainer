namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>
/// Strategy for how health drains over the course of a run. Extracting this lets modes (and future
/// plugins) swap the pressure curve without touching the session loop.
/// </summary>
public interface IPressureModel
{
    /// <summary>Returns the new health after applying one frame of pressure.</summary>
    /// <param name="currentHealth">Health at the start of the frame.</param>
    /// <param name="deltaSeconds">Elapsed run time since the previous frame.</param>
    /// <param name="elapsedRunSeconds">Total elapsed run time so far.</param>
    /// <param name="settings">Active session settings (for <c>Pressure</c> and related tuning).</param>
    double Apply(double currentHealth, double deltaSeconds, double elapsedRunSeconds, AimSessionSettings settings);
}
