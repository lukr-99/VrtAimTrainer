using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.Workflow;

public sealed record TrainingPreset(
    string Id,
    string Name,
    string Description,
    string GamemodeId,
    AimSessionSettings Settings,
    bool IsBuiltIn);
