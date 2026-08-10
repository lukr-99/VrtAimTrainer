namespace VrtAimTrainer.Core.Features.Gameplay;

public interface IAimGamemode
{
    string Id { get; }

    string DisplayName { get; }

    string Description { get; }

    AimSessionSettings CreateDefaultSettings();

    IAimSessionController CreateController(int viewportWidth, int viewportHeight, AimSessionSettings settings, IRandomSource randomSource);

    /// <summary>
    /// Named tuning variants this mode ships (excluding the implicit "Default", which the host
    /// synthesizes from <see cref="CreateDefaultSettings"/>). Plugin modes can override this to ship
    /// their own presets; the default is none.
    /// </summary>
    IReadOnlyList<GamemodePreset> CreatePresets() => [];
}
