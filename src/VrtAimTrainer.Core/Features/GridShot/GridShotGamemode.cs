using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.GridShot;

public sealed class GridShotGamemode : IAimGamemode
{
    private readonly AimSessionSettings _defaultSettings;

    public GridShotGamemode(string id, string displayName, string description, AimSessionSettings defaultSettings)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        _defaultSettings = defaultSettings.Normalize();
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public AimSessionSettings CreateDefaultSettings()
    {
        return _defaultSettings;
    }

    public IAimSessionController CreateController(int viewportWidth, int viewportHeight, AimSessionSettings settings, IRandomSource randomSource)
    {
        return new AimSessionEngine(
            Id,
            DisplayName,
            viewportWidth,
            viewportHeight,
            settings,
            new GridTargetProvider(randomSource),
            new LogarithmicPressureModel(),
            new CheeseCorrectedScoringRule());
    }
}
