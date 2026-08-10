namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>
/// The single, fully data-defined game mode. A mode is now just its identity, default settings,
/// presets, and a choice of strategies — no per-mode subclass. It builds an <see cref="AimSessionEngine"/>
/// wired with the mode's <see cref="ITargetProvider"/> (via a factory, since providers hold per-run
/// state and need the session's random source), pressure model, and scoring rule.
/// <para>
/// This replaces the previous inconsistency where some modes were bespoke subclasses (BallSheet,
/// VRT Speed) and others were parameterized instances (GridShot). Built-in modes are defined in
/// <c>BuiltInGamemodes</c>; plugins can construct these directly too.
/// </para>
/// </summary>
public sealed class AimGamemode : IAimGamemode
{
    private readonly AimSessionSettings _defaultSettings;
    private readonly IReadOnlyList<GamemodePreset> _presets;
    private readonly Func<IRandomSource, ITargetProvider> _targetProviderFactory;
    private readonly IPressureModel _pressureModel;
    private readonly IScoringRule _scoringRule;

    public AimGamemode(
        string id,
        string displayName,
        string description,
        AimSessionSettings defaultSettings,
        Func<IRandomSource, ITargetProvider> targetProviderFactory,
        IReadOnlyList<GamemodePreset>? presets = null,
        IPressureModel? pressureModel = null,
        IScoringRule? scoringRule = null)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        _defaultSettings = defaultSettings.Normalize();
        _targetProviderFactory = targetProviderFactory;
        _presets = presets ?? [];
        _pressureModel = pressureModel ?? new LogarithmicPressureModel();
        _scoringRule = scoringRule ?? new CheeseCorrectedScoringRule();
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public AimSessionSettings CreateDefaultSettings()
    {
        return _defaultSettings;
    }

    public IReadOnlyList<GamemodePreset> CreatePresets()
    {
        return _presets;
    }

    public IAimSessionController CreateController(int viewportWidth, int viewportHeight, AimSessionSettings settings, IRandomSource randomSource)
    {
        return new AimSessionEngine(
            Id,
            DisplayName,
            viewportWidth,
            viewportHeight,
            settings,
            _targetProviderFactory(randomSource),
            _pressureModel,
            _scoringRule);
    }
}
