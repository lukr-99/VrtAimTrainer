namespace VrtAimTrainer.Core.Features.Gameplay;

public interface IAimGamemodePlugin
{
    AimGamemodePluginMetadata Metadata { get; }

    IReadOnlyList<IAimGamemode> CreateGamemodes();
}
