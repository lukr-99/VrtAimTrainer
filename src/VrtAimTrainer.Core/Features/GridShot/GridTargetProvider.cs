using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.GridShot;

/// <summary>
/// Target provider for the GridShot modes: targets appear only on grid cell centres, with a
/// configurable number live at once; hitting one replaces it with a fresh cell. Distance/angle for
/// analytics are measured from the previously hit target, matching the original GridShot loop.
/// </summary>
public sealed class GridTargetProvider : ITargetProvider
{
    private readonly GridShotCellLayoutBuilder _cellLayoutBuilder = new();
    private readonly GridShotCellSelector _cellSelector = new();
    private readonly IRandomSource _randomSource;
    private readonly List<int> _activeCellIndices = [];
    private IReadOnlyList<AimPoint> _cellCenters = [];
    private AimSessionSettings _settings = null!; // assigned in Initialize before any other use
    private TargetSnapshot[] _activeTargets = [];
    private AimPoint _lastHitPosition;
    private bool _hasLastHit;

    public GridTargetProvider(IRandomSource randomSource)
    {
        _randomSource = randomSource;
    }

    public IReadOnlyList<TargetSnapshot> ActiveTargets => _activeTargets;

    public void Initialize(AimPlayfieldLayout layout, AimSessionSettings settings)
    {
        _settings = settings;
        _cellCenters = _cellLayoutBuilder.Build(layout, settings);

        // Deterministic placeholder (no randomness) so the engine can build state; Reset() seeds for real.
        _activeCellIndices.Clear();
        var placeholderCount = Math.Clamp(settings.EffectiveConcurrentTargetCount, 1, Math.Max(1, _cellCenters.Count));
        for (var index = 0; index < placeholderCount; index++)
        {
            _activeCellIndices.Add(index);
        }

        SyncTargetsFromActiveCells();
    }

    public void Reconfigure(AimPlayfieldLayout layout, AimSessionSettings settings)
    {
        _settings = settings;
        _cellCenters = _cellLayoutBuilder.Build(layout, settings);
        TrimActiveCellsToGrid();
        SyncTargetsFromActiveCells();
    }

    public void Reset()
    {
        _hasLastHit = false;
        SeedActiveTargets();
    }

    public void BeginRun()
    {
        _hasLastHit = false;
        SeedActiveTargets();
    }

    public TargetHit? TryRegisterHit(AimPoint cursor)
    {
        var hitIndex = FindHitTargetIndex(cursor);
        if (hitIndex < 0)
        {
            return null;
        }

        var hitTarget = _activeTargets[hitIndex];
        var distanceFromPrevious = _hasLastHit ? AimPoint.Distance(_lastHitPosition, hitTarget.Position) : 0d;
        var angleFromPrevious = _hasLastHit ? AimGeometry.AngleDegreesBetween(_lastHitPosition, hitTarget.Position) : null;

        ReplaceHitTarget(hitIndex);
        _lastHitPosition = hitTarget.Position;
        _hasLastHit = true;

        return new TargetHit(hitTarget.Position, distanceFromPrevious, angleFromPrevious);
    }

    private void SeedActiveTargets()
    {
        _activeCellIndices.Clear();
        _activeCellIndices.AddRange(_cellSelector.CreateInitialSelection(
            _cellCenters.Count,
            _settings.EffectiveConcurrentTargetCount,
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

        var maxConcurrentCount = Math.Min(maxCellCount, _settings.EffectiveConcurrentTargetCount);
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

        _activeTargets = _activeCellIndices
            .Select(cellIndex => new TargetSnapshot(_cellCenters[cellIndex], _settings.TargetDiameter))
            .ToArray();
    }

    private int FindHitTargetIndex(AimPoint cursor)
    {
        for (var index = 0; index < _activeTargets.Length; index++)
        {
            if (AimTargetHitTester.IsCursorInsideTarget(cursor, _activeTargets[index], _settings))
            {
                return index;
            }
        }

        return -1;
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
}
