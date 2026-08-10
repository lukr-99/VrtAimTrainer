using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.BallSheetClassic;

public sealed class BallSheetClassicGamemodePlugin : IAimGamemodePlugin
{
    public AimGamemodePluginMetadata Metadata { get; } = new(
        PluginId: "vrtaimtrainer.builtin.ballsheet",
        DisplayName: "VRT Built-In Modes",
        Version: "0.5.2",
        Author: "VrtAimTrainer",
        Description: "Bundled pressure-driven reactive aim modes shipped with the desktop app.");

    public IReadOnlyList<IAimGamemode> CreateGamemodes()
    {
        return BuiltInGamemodes.All();
    }
}
