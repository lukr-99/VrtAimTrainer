namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>
/// The catalog of tunable <see cref="AimSessionSettings"/> parameters. This is the single source of
/// truth the settings UI renders from; adding a new tunable means adding one entry here (and a field
/// on <see cref="AimSessionSettings"/>) rather than editing page code.
/// </summary>
public static class AimSettingsSchema
{
    public static IReadOnlyList<SettingParameter> Parameters { get; } =
    [
        // --- Run tuning (relevant to every mode) ---
        new SettingParameter(
            "cursor-size", "Cursor diameter", SettingGroup.Run, 5f, 100f, "0.0",
            settings => settings.CursorDiameter,
            (settings, value) => settings with { CursorDiameter = value }),
        new SettingParameter(
            "score-per-hit", "Score per hit", SettingGroup.Run, 5f, 150f, "0.0",
            settings => (float)settings.ScorePerHit,
            (settings, value) => settings with { ScorePerHit = value }),
        new SettingParameter(
            "pressure", "Pressure", SettingGroup.Run, 5f, 150f, "0.0",
            settings => (float)settings.Pressure,
            (settings, value) => settings with { Pressure = value }),
        new SettingParameter(
            "starting-health", "Starting HP", SettingGroup.Run, 20f, 250f, "0.0",
            settings => (float)settings.StartingHealth,
            (settings, value) => settings with { StartingHealth = value }),
        new SettingParameter(
            "round-time", "Round time (s)", SettingGroup.Run, 10f, 180f, "0",
            settings => (float)(settings.FixedDurationSeconds ?? 45d),
            (settings, value) => settings with { FixedDurationSeconds = value }),
        new SettingParameter(
            "cheese-threshold", "Cheese threshold (ms)", SettingGroup.Run, 2f, 250f, "0.0",
            settings => (float)settings.CheeseThresholdMs,
            (settings, value) => settings with { CheeseThresholdMs = value }),

        // --- Geometry / layout ---
        new SettingParameter(
            "sheet-width", "Sheet width", SettingGroup.Geometry, 700f, 1800f, "0",
            settings => settings.EffectiveSheetWidth,
            (settings, value) => settings with { SheetWidth = value, ArenaSize = MathF.Max(value, settings.EffectiveSheetHeight) }),
        new SettingParameter(
            "sheet-height", "Sheet height", SettingGroup.Geometry, 700f, 1800f, "0",
            settings => settings.EffectiveSheetHeight,
            (settings, value) => settings with { SheetHeight = value, ArenaSize = MathF.Max(settings.EffectiveSheetWidth, value) }),
        new SettingParameter(
            "target-size", "Ball size", SettingGroup.Geometry, 10f, 140f, "0.0",
            settings => settings.TargetDiameter,
            (settings, value) => settings with { TargetDiameter = value }),
        new SettingParameter(
            "spawn-safety", "Ball spread", SettingGroup.Geometry, 120f, 700f, "0",
            settings => settings.MinimumSpawnDistance,
            (settings, value) => settings with { MinimumSpawnDistance = value },
            AppliesWhen: settings => !settings.UsesGrid),
        new SettingParameter(
            "spawn-inset", "Edge padding", SettingGroup.Geometry, 0.05f, 0.30f, "0.00",
            settings => settings.SpawnInsetRatio,
            (settings, value) => settings with { SpawnInsetRatio = value }),
        new SettingParameter(
            "spawn-top", "Top safe padding", SettingGroup.Geometry, 0f, 160f, "0",
            settings => settings.SpawnTopPadding,
            (settings, value) => settings with { SpawnTopPadding = value }),
        new SettingParameter(
            "spawn-bottom", "Bottom safe padding", SettingGroup.Geometry, 0f, 180f, "0",
            settings => settings.SpawnBottomPadding,
            (settings, value) => settings with { SpawnBottomPadding = value }),
        new SettingParameter(
            "grid-rows", "Grid rows", SettingGroup.Geometry, 2f, 8f, "0",
            settings => settings.GridRows,
            (settings, value) => settings with { GridRows = (int)MathF.Round(value) },
            AppliesWhen: settings => settings.UsesGrid),
        new SettingParameter(
            "grid-columns", "Grid columns", SettingGroup.Geometry, 2f, 8f, "0",
            settings => settings.GridColumns,
            (settings, value) => settings with { GridColumns = (int)MathF.Round(value) },
            AppliesWhen: settings => settings.UsesGrid),
        new SettingParameter(
            "grid-concurrent", "Live targets", SettingGroup.Geometry, 1f, 9f, "0",
            settings => settings.ConcurrentTargetCount,
            (settings, value) => settings with { ConcurrentTargetCount = (int)MathF.Round(value) },
            AppliesWhen: settings => settings.UsesGrid),
    ];

    /// <summary>Parameters in <paramref name="group"/> that are relevant for the given settings.</summary>
    public static IEnumerable<SettingParameter> ApplicableParameters(SettingGroup group, AimSessionSettings settings)
    {
        return Parameters.Where(parameter => parameter.Group == group && parameter.AppliesTo(settings));
    }
}
