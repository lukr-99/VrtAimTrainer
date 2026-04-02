using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed record TrainerAppState(
    string ActiveGamemodeId,
    AimSessionSettings ActiveSettings,
    string DraftGamemodeId,
    string? SelectedPresetId,
    AimSessionSettings DraftSettings,
    string ActiveDisplayProfileId,
    string AnalyticsGamemodeFilterId = "",
    MouseSettings? MouseSettings = null);
