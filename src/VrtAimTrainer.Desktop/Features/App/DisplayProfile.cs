namespace VrtAimTrainer.Desktop.Features.App;

public sealed record DisplayProfile(
    string Id,
    string Label,
    string Description,
    int? Width,
    int? Height,
    bool IsFullscreen);
