using VrtAimTrainer.Core.Features.Analytics;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.GridShot;

public sealed class GridShotSessionController : IAimSessionController
{
    private readonly SessionAnalyticsCalculator _analyticsCalculator = new();
    private readonly GridShotCellLayoutBuilder _cellLayoutBuilder = new();
    private readonly GridShotCellSelector _cellSelector = new();
    private readonly AimPlayfieldLayoutCalculator _layoutCalculator = new();
    private readonly IRandomSource _randomSource;
    private readonly List<int> _activeCellIndices = [];
    private readonly List<HitSample> _samples = [];
    private IReadOnlyList<AimPoint> _cellCenters = [];
    private AimPoint _lastCursorPosition;
    private AimPoint _lastHitTargetPosition;
    private double _cursorTravelSinceLastHit;
    private double _lastFrameRunTimeSeconds;
    private double _lastHitRunTimeSeconds;
    private double _runStartedAtTimestamp;
    private bool _hasLastCursorPosition;
    private bool _hasLastHitTargetPosition;

    public GridShotSessionController(
        string gamemodeId,
        string displayName,
        int viewportWidth,
        int viewportHeight,
        AimSessionSettings settings,
        IRandomSource randomSource)
    {
        GamemodeId = gamemodeId;
        DisplayName = displayName;
        _randomSource = randomSource;

        var normalizedSettings = settings.Normalize();
        var layout = _layoutCalculator.Create(viewportWidth, viewportHeight, normalizedSettings);
        State = new AimSessionState(
            normalizedSettings,
            layout,
            new TargetSnapshot(layout.ViewportBounds.Center, normalizedSettings.TargetDiameter));

        RebuildCellCenters();
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
        RebuildCellCenters();
        SyncTargetsFromActiveCells();
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
        RebuildCellCenters();
        TrimActiveCellsToGrid();
        SyncTargetsFromActiveCells();

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

        if (State.Phase == SessionPhase.PostGame)
        {
            return;
        }

        if (State.Phase == SessionPhase.PreGame)
        {
            return;
        }

        var hitTargetIndex = FindHitTargetIndex(State.CursorPosition);
        if (hitTargetIndex < 0)
        {
            return;
        }

        var hitTarget = State.Targets[hitTargetIndex];
        RegisterHit(hitTarget);
        ReplaceHitTarget(hitTargetIndex);
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
        _activeCellIndices.Clear();
        _runStartedAtTimestamp = 0d;
        _lastHitRunTimeSeconds = 0d;
        _lastFrameRunTimeSeconds = 0d;
        _cursorTravelSinceLastHit = 0d;
        _hasLastCursorPosition = false;
        _hasLastHitTargetPosition = false;

        State.Phase = SessionPhase.PreGame;
        State.Health = State.Settings.StartingHealth;
        State.Score = 0d;
        State.ElapsedRunSeconds = 0d;
        State.HitCount = 0;
        State.LastReactionMs = null;
        State.AverageReactionMs = 0d;
        State.AverageHitsPerSecond = 0d;
        State.SettingsChangedDuringRun = false;
        State.LastCompletedResult = null;

        SeedActiveTargets();
    }

    private void RebuildCellCenters()
    {
        _cellCenters = _cellLayoutBuilder.Build(State.Layout, State.Settings);
    }

    private void SeedActiveTargets()
    {
        _activeCellIndices.Clear();
        _activeCellIndices.AddRange(_cellSelector.CreateInitialSelection(
            _cellCenters.Count,
            State.Settings.EffectiveConcurrentTargetCount,
            _randomSource));
        SyncTargetsFromActiveCells();
    }

    private void TrimActiveCellsToGrid()
    {
        if (_activeCellIndices.Count == 0)
        {
            return;
        }

        var maxCellCount = _cellCenters.Count;
        _activeCellIndices.RemoveAll(index => index < 0 || index >= maxCellCount);

        var maxConcurrentCount = Math.Min(maxCellCount, State.Settings.EffectiveConcurrentTargetCount);
        while (_activeCellIndices.Count > maxConcurrentCount)
        {
            _activeCellIndices.RemoveAt(_activeCellIndices.Count - 1);
        }

        while (_activeCellIndices.Count < maxConcurrentCount)
        {
            _activeCellIndices.Add(_cellSelector.PickReplacementCellIndex(
                maxCellCount,
                _activeCellIndices,
                fallbackCellIndex: 0,
                _randomSource));
        }
    }

    private void SyncTargetsFromActiveCells()
    {
        if (_activeCellIndices.Count == 0)
        {
            SeedActiveTargets();
            return;
        }

        var targets = _activeCellIndices
            .Select(cellIndex => new TargetSnapshot(_cellCenters[cellIndex], State.Settings.TargetDiameter))
            .ToArray();

        State.SetTargets(targets);
    }

    private int FindHitTargetIndex(AimPoint cursorPosition)
    {
        for (var index = 0; index < State.Targets.Count; index++)
        {
            if (AimTargetHitTester.IsCursorInsideTarget(cursorPosition, State.Targets[index], State.Settings))
            {
                return index;
            }
        }

        return -1;
    }

    private void StartRun(double timestampSeconds)
    {
        _runStartedAtTimestamp = timestampSeconds;
        _lastHitRunTimeSeconds = 0d;
        _lastFrameRunTimeSeconds = 0d;
        _cursorTravelSinceLastHit = 0d;
        _hasLastHitTargetPosition = false;
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

        SeedActiveTargets();
    }

    private void RegisterHit(TargetSnapshot hitTarget)
    {
        var reactionTimeSeconds = State.ElapsedRunSeconds - _lastHitRunTimeSeconds;
        var cheeseFactor = Math.Min(reactionTimeSeconds, State.Settings.CheeseThresholdSeconds) / State.Settings.CheeseThresholdSeconds;
        var awardedScore = State.Settings.ScorePerHit * cheeseFactor;
        var healthBeforeHit = State.Health;
        var scoreBeforeHit = State.Score;
        var distanceFromPreviousTarget = _hasLastHitTargetPosition
            ? AimPoint.Distance(_lastHitTargetPosition, hitTarget.Position)
            : 0d;
        var angleFromPreviousTarget = _hasLastHitTargetPosition
            ? CalculateAngleFromPreviousTarget(_lastHitTargetPosition, hitTarget.Position)
            : null;

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
            TargetPosition: hitTarget.Position,
            DistanceFromPreviousTarget: distanceFromPreviousTarget,
            CursorTravelDistance: _cursorTravelSinceLastHit,
            AngleFromPreviousTargetDegrees: angleFromPreviousTarget,
            HealthBeforeHit: healthBeforeHit,
            HealthAfterHit: State.Health,
            ScoreBeforeHit: scoreBeforeHit,
            ScoreAfterHit: State.Score));

        _lastHitRunTimeSeconds = State.ElapsedRunSeconds;
        _lastHitTargetPosition = hitTarget.Position;
        _hasLastHitTargetPosition = true;
        _cursorTravelSinceLastHit = 0d;
    }

    private void ReplaceHitTarget(int hitTargetIndex)
    {
        var removedCellIndex = _activeCellIndices[hitTargetIndex];
        _activeCellIndices.RemoveAt(hitTargetIndex);
        _activeCellIndices.Insert(
            hitTargetIndex,
            _cellSelector.PickReplacementCellIndex(
                _cellCenters.Count,
                _activeCellIndices,
                removedCellIndex,
                _randomSource));

        SyncTargetsFromActiveCells();
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
