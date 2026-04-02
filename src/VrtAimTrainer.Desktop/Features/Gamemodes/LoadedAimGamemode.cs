using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.Gamemodes;

public sealed record LoadedAimGamemode(
    IAimGamemode Gamemode,
    AimGamemodePluginMetadata PluginMetadata,
    string SourcePath,
    bool IsBuiltIn)
{
    public string Id => Gamemode.Id;

    public string DisplayName => Gamemode.DisplayName;

    public string Description => Gamemode.Description;
}
