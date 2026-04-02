namespace VrtAimTrainer.Core.Features.Gameplay;

public interface IAimGamemode
{
    string Id { get; }

    string DisplayName { get; }

    string Description { get; }

    AimSessionSettings CreateDefaultSettings();

    IAimSessionController CreateController(int viewportWidth, int viewportHeight, AimSessionSettings settings, IRandomSource randomSource);
}
