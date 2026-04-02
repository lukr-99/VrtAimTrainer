using VrtAimTrainer.Core.Features.Gameplay;
using VrtAimTrainer.Desktop.Features.Gamemodes;
using VrtAimTrainer.Desktop.Features.Workflow;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed class TrainerWorkflowState
{
    private readonly AimGamemodeCatalog _gamemodeCatalog;
    private readonly TrainingPresetCatalog _presetCatalog;

    public TrainerWorkflowState(AimGamemodeCatalog gamemodeCatalog, TrainingPresetCatalog presetCatalog)
    {
        _gamemodeCatalog = gamemodeCatalog;
        _presetCatalog = presetCatalog;

        ActiveGamemode = _gamemodeCatalog.GetDefault();
        DraftGamemode = ActiveGamemode;
        SelectedPreset = _presetCatalog.GetDefaultPreset(ActiveGamemode.Id);
        ActiveSettings = SelectedPreset.Settings.Normalize();
        DraftSettings = ActiveSettings;
    }

    public LoadedAimGamemode ActiveGamemode { get; private set; }

    public AimSessionSettings ActiveSettings { get; private set; }

    public LoadedAimGamemode DraftGamemode { get; private set; }

    public AimSessionSettings DraftSettings { get; private set; }

    public TrainingPreset? SelectedPreset { get; private set; }

    public void Restore(TrainerAppState appState)
    {
        ActiveGamemode = ResolveGamemode(appState.ActiveGamemodeId);
        DraftGamemode = ResolveGamemode(appState.DraftGamemodeId);
        ActiveSettings = appState.ActiveSettings.Normalize();
        DraftSettings = appState.DraftSettings.Normalize();
        SelectedPreset = ResolvePreset(appState.SelectedPresetId, DraftGamemode.Id, DraftSettings);
    }

    public TrainerAppState CreateAppState(string activeDisplayProfileId, string analyticsGamemodeFilterId, MouseSettings mouseSettings)
    {
        return new TrainerAppState(
            ActiveGamemode.Id,
            ActiveSettings.Normalize(),
            DraftGamemode.Id,
            SelectedPreset?.Id,
            DraftSettings.Normalize(),
            activeDisplayProfileId,
            analyticsGamemodeFilterId,
            mouseSettings.Normalize());
    }

    public LoadedAimGamemode ResolveGamemode(string? gamemodeId)
    {
        return _gamemodeCatalog.Gamemodes.FirstOrDefault(
                   gamemode => string.Equals(gamemode.Id, gamemodeId, StringComparison.OrdinalIgnoreCase))
               ?? _gamemodeCatalog.GetDefault();
    }

    public void ApplyDraftToLive()
    {
        ActiveGamemode = DraftGamemode;
        ActiveSettings = DraftSettings.Normalize();
        DraftSettings = ActiveSettings;
    }

    public void ResetDraftToLive()
    {
        DraftGamemode = ActiveGamemode;
        DraftSettings = ActiveSettings.Normalize();
        SelectedPreset = ResolvePreset(SelectedPreset?.Id, DraftGamemode.Id, DraftSettings);
    }

    public void ResetDraftToSelectedPreset()
    {
        SelectedPreset ??= _presetCatalog.GetDefaultPreset(DraftGamemode.Id);
        DraftSettings = SelectedPreset.Settings.Normalize();
    }

    public void ApplyPreset(TrainingPreset preset)
    {
        DraftGamemode = ResolveGamemode(preset.GamemodeId);
        SelectedPreset = preset;
        DraftSettings = preset.Settings.Normalize();
    }

    public void SelectGamemode(LoadedAimGamemode gamemode)
    {
        DraftGamemode = gamemode;
        SelectedPreset = _presetCatalog.GetDefaultPreset(gamemode.Id);
        DraftSettings = SelectedPreset.Settings.Normalize();
    }

    public void UpdateDraftSettings(AimSessionSettings settings)
    {
        DraftSettings = settings.Normalize();
    }

    public bool HasDraftChanges()
    {
        return !string.Equals(DraftGamemode.Id, ActiveGamemode.Id, StringComparison.OrdinalIgnoreCase)
               || DraftSettings.Normalize() != ActiveSettings.Normalize();
    }

    public bool IsDraftModifiedFromPreset()
    {
        return SelectedPreset is null || DraftSettings.Normalize() != SelectedPreset.Settings.Normalize();
    }

    public string GetDraftPresetLabel()
    {
        if (SelectedPreset is null)
        {
            return "Custom Draft";
        }

        return IsDraftModifiedFromPreset()
            ? $"{SelectedPreset.Name} (Modified)"
            : SelectedPreset.Name;
    }

    private TrainingPreset ResolvePreset(string? presetId, string gamemodeId, AimSessionSettings draftSettings)
    {
        return _presetCatalog.FindById(presetId, gamemodeId)
               ?? _presetCatalog.FindMatching(gamemodeId, draftSettings)
               ?? _presetCatalog.GetDefaultPreset(gamemodeId);
    }
}
