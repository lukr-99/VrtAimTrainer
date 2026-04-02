using VrtAimTrainer.Core.Features.Analytics;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.BallSheetClassic;

public sealed class BallSheetClassicSessionController : IAimSessionController
{
    private readonly SessionAnalyticsCalculator _analyticsCalculator = new();
    private readonly AimPlayfieldLayoutCalculator _layoutCalculator = new();
    private readonly IRandomSource _randomSource;
    private readonly BallSheetClassicTargetSpawner _targetSpawner = new();
    private readonly List<HitSample> _samples = [];
    private AimPoint _lastCursorPosition;
    private double _currentTargetDistanceFromPrevious;
    private double _currentTargetCursorTravelDistance;
    private double? _currentTargetAngleFromPreviousDegrees;
    private bool _hasLastCursorPosition;
    private double _lastFrameRunTimeSeconds;
    private double _lastHitRunTimeSeconds;
    private double _runStartedAtTimestamp;

    public BallSheetClassicSessionController(int viewportWidth, int viewportHeight, AimSessionSettings settings, IRandomSource randomSource)
    {
        _randomSource = randomSource;

        var normalizedSettings = settings.Normalize();
        var layout = _layoutCalculator.Create(viewportWidth, viewportHeight, normalizedSettings);
        State = new AimSessionState(
            normalizedSettings,
            layout,
            new TargetSnapshot(layout.ViewportBounds.Center, normalizedSettings.TargetDiameter));

        Reset();
    }

    public string GamemodeId => "ballsheet-classic";

    public string DisplayName => "BallSheet Classic";

    public AimSessionState State { get; }

    public IReadOnlyList<HitSample> Samples => _samples;

    public void ResizeViewport(int viewportWidth, int viewportHeight)
    {
        if ((int)State.Layout.ViewportBounds.Width == viewportWidth &&
            (int)State.Layout.ViewportBounds.Height == viewportHeight)
        {
            return;
        }

        var oldViewportCenter = State.Layout.ViewportBounds.Center;
        State.Layout = _layoutCalculator.Create(viewportWidth, viewportHeight, State.Settings);

        var newViewportCenter = State.Layout.ViewportBounds.Center;
        var translatedTarget = State.Target.Position.Translate(
            newViewportCenter.X - oldViewportCenter.X,
            newViewportCenter.Y - oldViewportCenter.Y);

        State.SetTarget(new TargetSnapshot(
            translatedTarget.Clamp(State.Layout.SpawnBounds),
            State.Settings.TargetDiameter));
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
        var oldViewportCenter = State.Layout.ViewportBounds.Center;

        State.Settings = normalizedSettings;
        State.Layout = _layoutCalculator.Create(viewportWidth, viewportHeight, normalizedSettings);

        var newViewportCenter = State.Layout.ViewportBounds.Center;
        var translatedTarget = State.Target.Position.Translate(
            newViewportCenter.X - oldViewportCenter.X,
            newViewportCenter.Y - oldViewportCenter.Y);

        State.SetTarget(new TargetSnapshot(
            translatedTarget.Clamp(State.Layout.SpawnBounds),
            normalizedSettings.TargetDiameter));

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

            State.Health -= deltaTime * State.Settings.Pressure * Math.Log(1d + State.ElapsedRunSeconds);
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

        if (State.Phase == SessionPhase.PostGame ||
            State.Phase == SessionPhase.PreGame ||
            !AimTargetHitTester.IsCursorInsideTarget(State.CursorPosition, State.Target, State.Settings))
        {
            return;
        }

        RegisterHit();

        var nextTarget = _targetSpawner.SpawnNextTarget(State.Target.Position, State.Layout, State.Settings, _randomSource);
        _currentTargetDistanceFromPrevious = nextTarget.DistanceFromPreviousTarget;
        ActivateNextTarget(nextTarget.Target);
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
        var nextTarget = _targetSpawner.SpawnNextTarget(State.Target.Position, State.Layout, State.Settings, _randomSource);
        _currentTargetDistanceFromPrevious = nextTarget.DistanceFromPreviousTarget;
        ActivateNextTarget(nextTarget.Target);
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
        _currentTargetAngleFromPreviousDegrees = null;
        _runStartedAtTimestamp = 0d;
        _lastHitRunTimeSeconds = 0d;
        _lastFrameRunTimeSeconds = 0d;
        _currentTargetDistanceFromPrevious = 0d;
        _currentTargetCursorTravelDistance = 0d;
        _hasLastCursorPosition = false;

        State.Phase = SessionPhase.PreGame;
        State.SetTarget(new TargetSnapshot(State.Layout.ViewportBounds.Center, State.Settings.TargetDiameter));
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

    private void RegisterHit()
    {
        var reactionTimeSeconds = State.ElapsedRunSeconds - _lastHitRunTimeSeconds;
        var cheeseFactor = Math.Min(reactionTimeSeconds, State.Settings.CheeseThresholdSeconds) / State.Settings.CheeseThresholdSeconds;
        var awardedScore = State.Settings.ScorePerHit * cheeseFactor;
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
            TargetPosition: State.Target.Position,
            DistanceFromPreviousTarget: _currentTargetDistanceFromPrevious,
            CursorTravelDistance: _currentTargetCursorTravelDistance,
            AngleFromPreviousTargetDegrees: _currentTargetAngleFromPreviousDegrees,
            HealthBeforeHit: healthBeforeHit,
            HealthAfterHit: State.Health,
            ScoreBeforeHit: scoreBeforeHit,
            ScoreAfterHit: State.Score));

        _lastHitRunTimeSeconds = State.ElapsedRunSeconds;
    }

    private void StartRun(double timestampSeconds)
    {
        _runStartedAtTimestamp = timestampSeconds;
        _lastHitRunTimeSeconds = 0d;
        _lastFrameRunTimeSeconds = 0d;
        _currentTargetCursorTravelDistance = 0d;
        _currentTargetAngleFromPreviousDegrees = null;
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

    private void TrackCursorMovement(in AimFrameInput input)
    {
        if (State.Phase == SessionPhase.InGame && _hasLastCursorPosition)
        {
            _currentTargetCursorTravelDistance += AimPoint.Distance(_lastCursorPosition, input.CursorPosition);
        }

        _lastCursorPosition = input.CursorPosition;
        _hasLastCursorPosition = true;
    }

    private void ActivateNextTarget(TargetSnapshot nextTarget)
    {
        _currentTargetAngleFromPreviousDegrees = CalculateAngleFromPreviousTarget(State.Target.Position, nextTarget.Position);
        _currentTargetCursorTravelDistance = 0d;
        State.SetTarget(nextTarget);
    }

    private bool HasFixedDurationExpired()
    {
        return State.Settings.FixedDurationSeconds is double fixedDurationSeconds &&
               State.ElapsedRunSeconds >= fixedDurationSeconds;
    }

    private static double? CalculateAngleFromPreviousTarget(AimPoint previousTarget, AimPoint nextTarget)
    {
        var deltaX = nextTarget.X - previousTarget.X;
        var deltaY = nextTarget.Y - previousTarget.Y;
        if (Math.Abs(deltaX) < float.Epsilon && Math.Abs(deltaY) < float.Epsilon)
        {
            return null;
        }

        var angleRadians = Math.Atan2(deltaY, deltaX);
        var angleDegrees = angleRadians * (180d / Math.PI);
        return angleDegrees < 0d ? angleDegrees + 360d : angleDegrees;
    }
}
