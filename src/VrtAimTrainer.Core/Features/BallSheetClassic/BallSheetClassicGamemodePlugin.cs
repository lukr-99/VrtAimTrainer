using VrtAimTrainer.Core.Features.Gameplay;
using VrtAimTrainer.Core.Features.GridShot;

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
        return
        [
            new BallSheetClassicGamemode(),
            new VrtSpeedGamemode(),
            new GridShotGamemode(
                id: "gridshot-3x3-quick",
                displayName: "3x3 Quick",
                description: "Fixed-time 3x3 drill with a single live target for fast lane changes.",
                defaultSettings: new AimSessionSettings(
                    CursorDiameter: 30f,
                    TargetDiameter: 62f,
                    ScorePerHit: 32d,
                    Pressure: 56d,
                    StartingHealth: 100d,
                    CheeseThresholdMs: 90d,
                    ArenaSize: 1100f,
                    SpawnInsetRatio: 0.16f,
                    SpawnTopPadding: 24f,
                    SpawnBottomPadding: 48f,
                    MinimumSpawnDistance: 0f,
                    SheetWidth: 1100f,
                    SheetHeight: 1100f,
                    FixedDurationSeconds: 30d,
                    GridRows: 3,
                    GridColumns: 3,
                    ConcurrentTargetCount: 1)),
            new GridShotGamemode(
                id: "gridshot-5x5-quick",
                displayName: "5x5 Quick",
                description: "Fixed-time 5x5 precision drill with one live target and tighter transitions.",
                defaultSettings: new AimSessionSettings(
                    CursorDiameter: 30f,
                    TargetDiameter: 48f,
                    ScorePerHit: 30d,
                    Pressure: 58d,
                    StartingHealth: 100d,
                    CheeseThresholdMs: 85d,
                    ArenaSize: 1200f,
                    SpawnInsetRatio: 0.12f,
                    SpawnTopPadding: 24f,
                    SpawnBottomPadding: 48f,
                    MinimumSpawnDistance: 0f,
                    SheetWidth: 1200f,
                    SheetHeight: 1200f,
                    FixedDurationSeconds: 30d,
                    GridRows: 5,
                    GridColumns: 5,
                    ConcurrentTargetCount: 1)),
            new GridShotGamemode(
                id: "gridshot-5x5-triple",
                displayName: "5x5 Triple",
                description: "Fixed-time 5x5 VRT grid with three live targets at once for faster target selection.",
                defaultSettings: new AimSessionSettings(
                    CursorDiameter: 30f,
                    TargetDiameter: 48f,
                    ScorePerHit: 32d,
                    Pressure: 60d,
                    StartingHealth: 100d,
                    CheeseThresholdMs: 90d,
                    ArenaSize: 1200f,
                    SpawnInsetRatio: 0.12f,
                    SpawnTopPadding: 24f,
                    SpawnBottomPadding: 48f,
                    MinimumSpawnDistance: 0f,
                    SheetWidth: 1200f,
                    SheetHeight: 1200f,
                    FixedDurationSeconds: 45d,
                    GridRows: 5,
                    GridColumns: 5,
                    ConcurrentTargetCount: 3)),
        ];
    }
}
