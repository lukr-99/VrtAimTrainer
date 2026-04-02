using VrtAimTrainer.Core.Features.BallSheetClassic;

namespace VrtAimTrainer.Core.Tests.Features.BallSheetClassic;

public sealed class BallSheetClassicGamemodePluginTests
{
    [Fact]
    public void CreateGamemodes_ExposesBuiltInModeCatalog()
    {
        var plugin = new BallSheetClassicGamemodePlugin();

        var gamemodes = plugin.CreateGamemodes();

        Assert.Equal(5, gamemodes.Count);
        Assert.Contains(gamemodes, gamemode => gamemode.Id == "ballsheet-classic");
        Assert.Contains(gamemodes, gamemode => gamemode.Id == "vrt-speed");
        Assert.Contains(gamemodes, gamemode => gamemode.Id == "gridshot-3x3-quick");
        Assert.Contains(gamemodes, gamemode => gamemode.Id == "gridshot-5x5-quick");
        Assert.Contains(gamemodes, gamemode => gamemode.Id == "gridshot-5x5-triple");
        Assert.Equal("VRT Built-In Modes", plugin.Metadata.DisplayName);
        Assert.Equal("vrtaimtrainer.builtin.ballsheet", plugin.Metadata.PluginId);
    }
}
