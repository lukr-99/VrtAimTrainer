using VrtAimTrainer.Core.Features.Gameplay;
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

            // Every mode gets an implicit "Default" preset synthesized from its default settings.
            presets.Add(new TrainingPreset(
                Id: $"{gamemode.Id}.default",
                Name: $"{gamemode.DisplayName} Default",
                Description: "Balanced baseline tuned to the mode author's default settings.",
                GamemodeId: gamemode.Id,
                Settings: defaultSettings,
                IsBuiltIn: gamemode.IsBuiltIn));

            // Variant presets now come from the gamemode itself (built-in or plugin), so plugins can
            // ship their own presets and there is no host-side per-mode hardcoding.
            foreach (var preset in gamemode.Gamemode.CreatePresets())
            {
                presets.Add(new TrainingPreset(
                    Id: $"{gamemode.Id}.{preset.Id}",
                    Name: preset.Name,
                    Description: preset.Description,
                    GamemodeId: gamemode.Id,
                    Settings: preset.Settings.Normalize(),
                    IsBuiltIn: gamemode.IsBuiltIn));
            }
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

    public TrainingPreset? FindMatching(string gamemodeId, AimSessionSettings settings)
    {
        var normalizedSettings = settings.Normalize();
        return GetPresetsForGamemode(gamemodeId)
            .FirstOrDefault(preset => preset.Settings.Normalize() == normalizedSettings);
    }
}
