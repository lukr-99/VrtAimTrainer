using VrtAimTrainer.Core.Features.Analytics;

namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>
/// The single session controller shared by every game mode. It owns run lifecycle, timing, health,
/// scoring, reaction/analytics bookkeeping, and cursor-travel tracking, and delegates the
/// mode-specific parts to an <see cref="ITargetProvider"/>, an <see cref="IPressureModel"/>, and an
/// <see cref="IScoringRule"/>. Replaces the previously duplicated per-mode session controllers.
/// </summary>
public sealed class AimSessionEngine : IAimSessionController
{
    private readonly SessionAnalyticsCalculator _analyticsCalculator = new();
    private readonly AimPlayfieldLayoutCalculator _layoutCalculator = new();
    private readonly ITargetProvider _targetProvider;
    private readonly IPressureModel _pressureModel;
    private readonly IScoringRule _scoringRule;
    private readonly List<HitSample> _samples = [];
    private AimPoint _lastCursorPosition;
    private double _cursorTravelSinceLastHit;
    private double _lastFrameRunTimeSeconds;
    private double _lastHitRunTimeSeconds;
    private double _runStartedAtTimestamp;
    private bool _hasLastCursorPosition;

    public AimSessionEngine(
        string gamemodeId,
        string displayName,
        int viewportWidth,
        int viewportHeight,
        AimSessionSettings settings,
        ITargetProvider targetProvider,
        IPressureModel pressureModel,
        IScoringRule scoringRule)
    {
        GamemodeId = gamemodeId;
        DisplayName = displayName;
        _targetProvider = targetProvider;
        _pressureModel = pressureModel;
        _scoringRule = scoringRule;

        var normalizedSettings = settings.Normalize();
        var layout = _layoutCalculator.Create(viewportWidth, viewportHeight, normalizedSettings);
        _targetProvider.Initialize(layout, normalizedSettings);
        State = new AimSessionState(normalizedSettings, layout, _targetProvider.ActiveTargets[0]);
        State.SetTargets(_targetProvider.ActiveTargets);

        Reset();
    }

    public string GamemodeId { get; }

    public string DisplayName { get; }

    public AimSessionState State { get; }

    public IReadOnlyList<HitSample> Samples => _samples;

    public void ResizeViewport(int viewportWidth, int viewportHeight)
    {
        if ((int)State.Layout.ViewportBounds.Width == viewportWidth &&
            (int)State.Layout.ViewportBounds.Height == viewportHeight)
        {
            return;
        }

        State.Layout = _layoutCalculator.Create(viewportWidth, viewportHeight, State.Settings);
        _targetProvider.Reconfigure(State.Layout, State.Settings);
        State.SetTargets(_targetProvider.ActiveTargets);
    }

    public void UpdateSettings(AimSessionSettings settings)
    {
        var normalizedSettings = settings.Normalize();
        if (State.Phase == SessionPhase.InGame && normalizedSettings != State.Settings)
        {
            State.SettingsChangedDuringRun = true;
        }

        var viewportWidth = (int)State.Layout.ViewportBounds.Width;
        var viewportHeight = (int)State.Layout.ViewportBounds.Height;

        State.Settings = normalizedSettings;
        State.Layout = _layoutCalculator.Create(viewportWidth, viewportHeight, normalizedSettings);
        _targetProvider.Reconfigure(State.Layout, normalizedSettings);
        State.SetTargets(_targetProvider.ActiveTargets);

        if (State.Phase != SessionPhase.InGame)
        {
            State.Health = normalizedSettings.StartingHealth;
        }
    }

    public void Update(in AimFrameInput input)
    {
        TrackCursorMovement(input);
        State.CursorPosition = input.CursorPosition;

        State.ElapsedRunSeconds = State.Phase == SessionPhase.InGame
            ? Math.Max(0d, input.TimestampSeconds - _runStartedAtTimestamp)
            : 0d;

        if (State.Phase == SessionPhase.InGame)
        {
            if (HasFixedDurationExpired())
            {
                Finish(input.TimestampSeconds);
                return;
            }

            var deltaTime = State.ElapsedRunSeconds - _lastFrameRunTimeSeconds;
            _lastFrameRunTimeSeconds = State.ElapsedRunSeconds;

            State.Health = _pressureModel.Apply(State.Health, deltaTime, State.ElapsedRunSeconds, State.Settings);
            if (State.Settings.UsesFixedDuration)
            {
                State.Health = Math.Max(0d, State.Health);
            }
            else if (State.Health < 0d)
            {
                Finish(input.TimestampSeconds);
                return;
            }
        }

        if (State.Phase != SessionPhase.InGame)
        {
            return;
        }

        var hit = _targetProvider.TryRegisterHit(State.CursorPosition);
        if (hit is null)
        {
            return;
        }

        RegisterHit(hit.Value);
        State.SetTargets(_targetProvider.ActiveTargets);
    }

    public bool Start(double timestampSeconds)
    {
        if (State.Phase == SessionPhase.InGame)
        {
            return false;
        }

        if (State.Phase == SessionPhase.PostGame)
        {
            Reset();
        }

        StartRun(timestampSeconds);
        _targetProvider.BeginRun();
        State.SetTargets(_targetProvider.ActiveTargets);
        return true;
    }

    public bool Finish(double timestampSeconds)
    {
        if (State.Phase == SessionPhase.PostGame)
        {
            return false;
        }

        if (_samples.Count == 0)
        {
            Reset();
            return false;
        }

        State.ElapsedRunSeconds = Math.Max(0d, timestampSeconds - _runStartedAtTimestamp);
        State.LastCompletedResult = _analyticsCalculator.CreateResult(
            GamemodeId,
            DisplayName,
            State.Settings,
            _samples,
            State.Score,
            State.ElapsedRunSeconds,
            State.SettingsChangedDuringRun);

        State.Phase = SessionPhase.PostGame;
        return true;
    }

    public void Reset()
    {
        _samples.Clear();
        _runStartedAtTimestamp = 0d;
        _lastHitRunTimeSeconds = 0d;
        _lastFrameRunTimeSeconds = 0d;
        _cursorTravelSinceLastHit = 0d;
        _hasLastCursorPosition = false;

        State.Phase = SessionPhase.PreGame;
        _targetProvider.Reset();
        State.SetTargets(_targetProvider.ActiveTargets);
        State.Health = State.Settings.StartingHealth;
        State.Score = 0d;
        State.ElapsedRunSeconds = 0d;
        State.HitCount = 0;
        State.LastReactionMs = null;
        State.AverageReactionMs = 0d;
        State.AverageHitsPerSecond = 0d;
        State.SettingsChangedDuringRun = false;
        State.LastCompletedResult = null;
    }

    private void StartRun(double timestampSeconds)
    {
        _runStartedAtTimestamp = timestampSeconds;
        _lastHitRunTimeSeconds = 0d;
        _lastFrameRunTimeSeconds = 0d;
        _cursorTravelSinceLastHit = 0d;

        State.Phase = SessionPhase.InGame;
        State.Score = 0d;
        State.Health = State.Settings.StartingHealth;
        State.ElapsedRunSeconds = 0d;
        State.HitCount = 0;
        State.LastReactionMs = null;
        State.AverageReactionMs = 0d;
        State.AverageHitsPerSecond = 0d;
        State.SettingsChangedDuringRun = false;
        State.LastCompletedResult = null;
    }

    private void RegisterHit(TargetHit hit)
    {
        var reactionTimeSeconds = State.ElapsedRunSeconds - _lastHitRunTimeSeconds;
        var awardedScore = _scoringRule.Award(reactionTimeSeconds, State.Settings);
        var healthBeforeHit = State.Health;
        var scoreBeforeHit = State.Score;

        State.Score += awardedScore;
        State.Health += awardedScore;
        State.HitCount = _samples.Count + 1;
        State.LastReactionMs = reactionTimeSeconds * 1000d;
        State.AverageReactionMs = _samples.Count == 0
            ? State.LastReactionMs.Value
            : (_samples.Sum(sample => sample.ReactionMilliseconds) + State.LastReactionMs.Value) / State.HitCount;
        State.AverageHitsPerSecond = State.AverageReactionMs <= 0d ? 0d : 1000d / State.AverageReactionMs;

        _samples.Add(new HitSample(
            ElapsedSeconds: State.ElapsedRunSeconds,
            ReactionSeconds: reactionTimeSeconds,
            CheeseCorrectedScore: awardedScore,
            TargetPosition: hit.Position,
            DistanceFromPreviousTarget: hit.DistanceFromPreviousTarget,
            CursorTravelDistance: _cursorTravelSinceLastHit,
            AngleFromPreviousTargetDegrees: hit.AngleFromPreviousTargetDegrees,
            HealthBeforeHit: healthBeforeHit,
            HealthAfterHit: State.Health,
            ScoreBeforeHit: scoreBeforeHit,
            ScoreAfterHit: State.Score));

        _lastHitRunTimeSeconds = State.ElapsedRunSeconds;
        _cursorTravelSinceLastHit = 0d;
    }

    private void TrackCursorMovement(in AimFrameInput input)
    {
        if (State.Phase == SessionPhase.InGame && _hasLastCursorPosition)
        {
            _cursorTravelSinceLastHit += AimPoint.Distance(_lastCursorPosition, input.CursorPosition);
        }

        _lastCursorPosition = input.CursorPosition;
        _hasLastCursorPosition = true;
    }

    private bool HasFixedDurationExpired()
    {
        return State.Settings.FixedDurationSeconds is double fixedDurationSeconds &&
               State.ElapsedRunSeconds >= fixedDurationSeconds;
    }
}
