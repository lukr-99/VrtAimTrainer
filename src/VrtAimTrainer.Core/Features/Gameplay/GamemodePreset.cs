namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>
/// A named tuning of a gamemode's settings, shipped by the gamemode itself (built-in or plugin).
/// The host prefixes <see cref="Id"/> with the owning gamemode id to form a globally unique preset id
/// and synthesizes an implicit "Default" preset from <see cref="IAimGamemode.CreateDefaultSettings"/>,
/// so a mode only lists its variants here.
/// </summary>
public sealed record GamemodePreset(
    string Id,
    string Name,
    string Description,
    AimSessionSettings Settings);
