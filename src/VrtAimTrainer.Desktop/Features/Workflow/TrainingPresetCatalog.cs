using VrtAimTrainer.Desktop.Features.Gamemodes;

namespace VrtAimTrainer.Desktop.Features.Workflow;

public sealed class TrainingPresetCatalog
{
    public TrainingPresetCatalog(IReadOnlyList<TrainingPreset> presets)
    {
        Presets = presets;
    }

    public IReadOnlyList<TrainingPreset> Presets { get; }

    public static TrainingPresetCatalog Create(IReadOnlyList<LoadedAimGamemode> gamemodes)
    {
        var presets = new List<TrainingPreset>();

        foreach (var gamemode in gamemodes)
        {
            var defaultSettings = gamemode.Gamemode.CreateDefaultSettings().Normalize();
            presets.Add(CreateDefaultPreset(gamemode, defaultSettings));
            presets.AddRange(CreateBuiltInPresets(gamemode.Id, defaultSettings));
        }

        return new TrainingPresetCatalog(
            presets
                .OrderBy(preset => preset.GamemodeId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(preset => preset.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }

    public IReadOnlyList<TrainingPreset> GetPresetsForGamemode(string gamemodeId)
    {
        return Presets
            .Where(preset => string.Equals(preset.GamemodeId, gamemodeId, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public TrainingPreset GetDefaultPreset(string gamemodeId)
    {
        return GetPresetsForGamemode(gamemodeId).First();
    }

    public TrainingPreset? FindById(string? presetId, string gamemodeId)
    {
        if (string.IsNullOrWhiteSpace(presetId))
        {
            return null;
        }

        return GetPresetsForGamemode(gamemodeId)
            .FirstOrDefault(preset => string.Equals(preset.Id, presetId, StringComparison.OrdinalIgnoreCase));
    }

    public TrainingPreset? FindMatching(string gamemodeId, VrtAimTrainer.Core.Features.Gameplay.AimSessionSettings settings)
    {
        var normalizedSettings = settings.Normalize();
        return GetPresetsForGamemode(gamemodeId)
            .FirstOrDefault(preset => preset.Settings.Normalize() == normalizedSettings);
    }

    private static TrainingPreset CreateDefaultPreset(LoadedAimGamemode gamemode, VrtAimTrainer.Core.Features.Gameplay.AimSessionSettings defaultSettings)
    {
        return new TrainingPreset(
            Id: $"{gamemode.Id}.default",
            Name: $"{gamemode.DisplayName} Default",
            Description: "Balanced baseline tuned to the mode author's default settings.",
            GamemodeId: gamemode.Id,
            Settings: defaultSettings,
            IsBuiltIn: true);
    }

    private static IReadOnlyList<TrainingPreset> CreateBuiltInPresets(
        string gamemodeId,
        VrtAimTrainer.Core.Features.Gameplay.AimSessionSettings defaultSettings)
    {
        return gamemodeId.ToLowerInvariant() switch
        {
            "ballsheet-classic" => CreateBallSheetClassicPresets(defaultSettings),
            "vrt-speed" => CreateVrtSpeedPresets(defaultSettings),
            "gridshot-3x3-quick" => CreateGridShot3x3QuickPresets(defaultSettings),
            "gridshot-5x5-quick" => CreateGridShot5x5QuickPresets(defaultSettings),
            "gridshot-5x5-triple" => CreateGridShot5x5TriplePresets(defaultSettings),
            _ => [],
        };
    }

    private static IReadOnlyList<TrainingPreset> CreateBallSheetClassicPresets(
        VrtAimTrainer.Core.Features.Gameplay.AimSessionSettings defaultSettings)
    {
        return
        [
            new TrainingPreset(
                Id: "ballsheet-classic.vrt-reactive-wide",
                Name: "VRT Reactive Wide",
                Description: "Wide-sheet reactive reps with a little more spread and tempo.",
                GamemodeId: "ballsheet-classic",
                Settings: defaultSettings with
                {
                    SheetWidth = 1450f,
                    SheetHeight = 980f,
                    ArenaSize = 1450f,
                    TargetDiameter = 58f,
                    MinimumSpawnDistance = 360f,
                    ScorePerHit = 38d,
                    Pressure = 58d,
                    CheeseThresholdMs = 110d,
                },
                IsBuiltIn: true),
            new TrainingPreset(
                Id: "ballsheet-classic.vrt-micro",
                Name: "Micro Corrections",
                Description: "Smaller targets and tighter punishment for cleaner micro-adjustments.",
                GamemodeId: "ballsheet-classic",
                Settings: defaultSettings with
                {
                    CursorDiameter = 26f,
                    TargetDiameter = 44f,
                    MinimumSpawnDistance = 340f,
                    Pressure = 64d,
                    CheeseThresholdMs = 90d,
                    SpawnInsetRatio = 0.14f,
                },
                IsBuiltIn: true),
            new TrainingPreset(
                Id: "ballsheet-classic.smoothness-lanes",
                Name: "Smoothness Lanes",
                Description: "Larger travel lanes with lower pressure for smoother full-sheet tracking.",
                GamemodeId: "ballsheet-classic",
                Settings: defaultSettings with
                {
                    SheetWidth = 1500f,
                    SheetHeight = 1040f,
                    ArenaSize = 1500f,
                    TargetDiameter = 62f,
                    MinimumSpawnDistance = 390f,
                    Pressure = 46d,
                    CheeseThresholdMs = 120d,
                },
                IsBuiltIn: true),
            new TrainingPreset(
                Id: "ballsheet-classic.vrt-endurance",
                Name: "Endurance Builder",
                Description: "Longer reactive runs with more health and slightly softer punishment.",
                GamemodeId: "ballsheet-classic",
                Settings: defaultSettings with
                {
                    StartingHealth = 150d,
                    Pressure = 44d,
                    TargetDiameter = 66f,
                    MinimumSpawnDistance = 280f,
                    CheeseThresholdMs = 125d,
                    FixedDurationSeconds = 60d,
                },
                IsBuiltIn: true),
        ];
    }

    private static IReadOnlyList<TrainingPreset> CreateVrtSpeedPresets(
        VrtAimTrainer.Core.Features.Gameplay.AimSessionSettings defaultSettings)
    {
        return
        [
            new TrainingPreset(
                Id: "vrt-speed.close-burst",
                Name: "Close Burst",
                Description: "Faster short-range spam for cleanup speed and tempo control.",
                GamemodeId: "vrt-speed",
                Settings: defaultSettings with
                {
                    TargetDiameter = 54f,
                    MinimumSpawnDistance = 150f,
                    Pressure = 74d,
                    CheeseThresholdMs = 72d,
                    FixedDurationSeconds = 25d,
                },
                IsBuiltIn: true),
            new TrainingPreset(
                Id: "vrt-speed.micro-rush",
                Name: "Micro Rush",
                Description: "Smaller targets and higher pace for close-quarters micro corrections.",
                GamemodeId: "vrt-speed",
                Settings: defaultSettings with
                {
                    CursorDiameter = 28f,
                    TargetDiameter = 46f,
                    MinimumSpawnDistance = 170f,
                    Pressure = 78d,
                    CheeseThresholdMs = 68d,
                    FixedDurationSeconds = 25d,
                },
                IsBuiltIn: true),
            new TrainingPreset(
                Id: "vrt-speed.stable-30",
                Name: "Stable 30",
                Description: "Slightly softer punishment with the full 30-second round for repeatable reps.",
                GamemodeId: "vrt-speed",
                Settings: defaultSettings with
                {
                    TargetDiameter = 58f,
                    MinimumSpawnDistance = 200f,
                    Pressure = 64d,
                    CheeseThresholdMs = 86d,
                },
                IsBuiltIn: true),
        ];
    }

    private static IReadOnlyList<TrainingPreset> CreateGridShot3x3QuickPresets(
        VrtAimTrainer.Core.Features.Gameplay.AimSessionSettings defaultSettings)
    {
        return
        [
            new TrainingPreset(
                Id: "gridshot-3x3-quick.snap-lines",
                Name: "Snap Lines",
                Description: "Bigger targets and shorter rounds for raw lane-switch speed.",
                GamemodeId: "gridshot-3x3-quick",
                Settings: defaultSettings with
                {
                    TargetDiameter = 68f,
                    Pressure = 60d,
                    FixedDurationSeconds = 25d,
                },
                IsBuiltIn: true),
            new TrainingPreset(
                Id: "gridshot-3x3-quick.control-hold",
                Name: "Control Hold",
                Description: "More precise 3x3 confirmations with a calmer tempo and smaller balls.",
                GamemodeId: "gridshot-3x3-quick",
                Settings: defaultSettings with
                {
                    TargetDiameter = 56f,
                    Pressure = 48d,
                    CheeseThresholdMs = 105d,
                    FixedDurationSeconds = 35d,
                },
                IsBuiltIn: true),
        ];
    }

    private static IReadOnlyList<TrainingPreset> CreateGridShot5x5QuickPresets(
        VrtAimTrainer.Core.Features.Gameplay.AimSessionSettings defaultSettings)
    {
        return
        [
            new TrainingPreset(
                Id: "gridshot-5x5-quick.precision-ladder",
                Name: "Precision Ladder",
                Description: "Smaller balls and a sharper threshold for clean 5x5 precision reps.",
                GamemodeId: "gridshot-5x5-quick",
                Settings: defaultSettings with
                {
                    CursorDiameter = 28f,
                    TargetDiameter = 42f,
                    Pressure = 62d,
                    CheeseThresholdMs = 72d,
                },
                IsBuiltIn: true),
            new TrainingPreset(
                Id: "gridshot-5x5-quick.tempo-builder",
                Name: "Tempo Builder",
                Description: "Slightly larger targets with more score throughput for pace building.",
                GamemodeId: "gridshot-5x5-quick",
                Settings: defaultSettings with
                {
                    TargetDiameter = 52f,
                    ScorePerHit = 34d,
                    Pressure = 54d,
                    FixedDurationSeconds = 35d,
                },
                IsBuiltIn: true),
        ];
    }

    private static IReadOnlyList<TrainingPreset> CreateGridShot5x5TriplePresets(
        VrtAimTrainer.Core.Features.Gameplay.AimSessionSettings defaultSettings)
    {
        return
        [
            new TrainingPreset(
                Id: "gridshot-5x5-triple.selective-chaos",
                Name: "Selective Chaos",
                Description: "Higher pressure triple-target work for faster target triage under load.",
                GamemodeId: "gridshot-5x5-triple",
                Settings: defaultSettings with
                {
                    Pressure = 66d,
                    CheeseThresholdMs = 78d,
                    FixedDurationSeconds = 40d,
                },
                IsBuiltIn: true),
            new TrainingPreset(
                Id: "gridshot-5x5-triple.endurance-grid",
                Name: "Endurance Grid",
                Description: "Longer triple-target blocks with larger targets to sustain accuracy under fatigue.",
                GamemodeId: "gridshot-5x5-triple",
                Settings: defaultSettings with
                {
                    TargetDiameter = 52f,
                    Pressure = 52d,
                    StartingHealth = 120d,
                    FixedDurationSeconds = 55d,
                },
                IsBuiltIn: true),
        ];
    }
}
