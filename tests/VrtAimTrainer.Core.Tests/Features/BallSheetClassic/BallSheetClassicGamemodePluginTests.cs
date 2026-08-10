using VrtAimTrainer.Core.Features.BallSheetClassic;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Tests.Features.BallSheetClassic;

public sealed class BallSheetClassicGamemodePluginTests
{
    [Fact]
    public void Gamemodes_ShipTheirOwnPresets()
    {
        var gamemodes = new BallSheetClassicGamemodePlugin()
            .CreateGamemodes()
            .ToDictionary(gamemode => gamemode.Id);

        // Presets are now owned by the modes themselves (no host-side switch).
        Assert.Equal(4, gamemodes["ballsheet-classic"].CreatePresets().Count);
        Assert.Equal(3, gamemodes["vrt-speed"].CreatePresets().Count);
        Assert.Equal(2, gamemodes["gridshot-3x3-quick"].CreatePresets().Count);
        Assert.Equal(2, gamemodes["gridshot-5x5-quick"].CreatePresets().Count);
        Assert.Equal(2, gamemodes["gridshot-5x5-triple"].CreatePresets().Count);

        var ballsheet = gamemodes["ballsheet-classic"];
        var defaults = ballsheet.CreateDefaultSettings().Normalize();
        Assert.All(
            ballsheet.CreatePresets(),
            preset => Assert.NotEqual(defaults, preset.Settings.Normalize()));
        Assert.Equal(
            ballsheet.CreatePresets().Count,
            ballsheet.CreatePresets().Select(preset => preset.Id).Distinct().Count());
    }

    [Fact]
    public void CreateController_ReportsMatchingGamemodeIdentity()
    {
        var plugin = new BallSheetClassicGamemodePlugin();

        foreach (var gamemode in plugin.CreateGamemodes())
        {
            var controller = gamemode.CreateController(
                1600,
                900,
                gamemode.CreateDefaultSettings(),
                new SystemRandomSource());

            // Regression guard: VRT Speed previously reused the BallSheet controller and
            // mislabeled its sessions as "ballsheet-classic".
            Assert.Equal(gamemode.Id, controller.GamemodeId);
            Assert.Equal(gamemode.DisplayName, controller.DisplayName);
        }
    }

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
