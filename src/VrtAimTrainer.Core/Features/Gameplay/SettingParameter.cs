namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>Which settings page a tunable parameter belongs to.</summary>
public enum SettingGroup
{
    /// <summary>Run tuning (scoring, pressure, health, round time, cheese threshold, cursor).</summary>
    Run,

    /// <summary>Playfield geometry and mode layout (sheet size, padding, spread, grid dimensions).</summary>
    Geometry,
}

/// <summary>
/// A data-driven description of one tunable field of <see cref="AimSessionSettings"/>: its range,
/// display, how to read/write it, and — crucially — when it is relevant. The UI renders sliders from
/// these descriptors instead of hardcoding them, so mode-inappropriate controls (e.g. ball spread on
/// a grid mode) disappear and mode-specific ones (grid rows/columns) appear automatically.
/// </summary>
public sealed record SettingParameter(
    string Key,
    string Label,
    SettingGroup Group,
    float Minimum,
    float Maximum,
    string Format,
    Func<AimSessionSettings, float> Get,
    Func<AimSessionSettings, float, AimSessionSettings> Set,
    Func<AimSessionSettings, bool>? AppliesWhen = null)
{
    /// <summary>Whether this parameter is relevant for the given settings (e.g. current mode).</summary>
    public bool AppliesTo(AimSessionSettings settings) => AppliesWhen?.Invoke(settings) ?? true;
}
