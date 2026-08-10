using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.BallSheetClassic;

/// <summary>
/// Target provider for the free-spawn reactive modes (BallSheet Classic, VRT Speed): a single target
/// that respawns anywhere on the sheet honouring the minimum-spawn-distance rule. Distance/angle for
/// analytics are measured from the previously active target, matching the original BallSheet loop.
/// </summary>
public sealed class FreeSpawnTargetProvider : ITargetProvider
{
    private readonly BallSheetClassicTargetSpawner _spawner = new();
    private readonly IRandomSource _randomSource;
    private AimSessionSettings _settings = null!; // assigned in Initialize before any other use
    private AimPlayfieldLayout _layout;
    private TargetSnapshot _currentTarget;
    private AimPoint _previousTargetPosition;
    private TargetSnapshot[] _activeTargets = [];

    public FreeSpawnTargetProvider(IRandomSource randomSource)
    {
        _randomSource = randomSource;
    }

    public IReadOnlyList<TargetSnapshot> ActiveTargets => _activeTargets;

    public void Initialize(AimPlayfieldLayout layout, AimSessionSettings settings)
    {
        _layout = layout;
        _settings = settings;
        SetCurrentTarget(new TargetSnapshot(layout.ViewportBounds.Center, settings.TargetDiameter));
        _previousTargetPosition = _currentTarget.Position;
    }

    public void Reconfigure(AimPlayfieldLayout layout, AimSessionSettings settings)
    {
        var oldCenter = _layout.ViewportBounds.Center;
        var newCenter = layout.ViewportBounds.Center;

        _layout = layout;
        _settings = settings;

        var translated = _currentTarget.Position
            .Translate(newCenter.X - oldCenter.X, newCenter.Y - oldCenter.Y)
            .Clamp(layout.SpawnBounds);
        SetCurrentTarget(new TargetSnapshot(translated, settings.TargetDiameter));
    }

    public void Reset()
    {
        SetCurrentTarget(new TargetSnapshot(_layout.ViewportBounds.Center, _settings.TargetDiameter));
        _previousTargetPosition = _currentTarget.Position;
    }

    public void BeginRun()
    {
        var from = _currentTarget.Position;
        var next = _spawner.SpawnNextTarget(from, _layout, _settings, _randomSource);
        _previousTargetPosition = from;
        SetCurrentTarget(next.Target);
    }

    public TargetHit? TryRegisterHit(AimPoint cursor)
    {
        if (!AimTargetHitTester.IsCursorInsideTarget(cursor, _currentTarget, _settings))
        {
            return null;
        }

        var hitPosition = _currentTarget.Position;
        var distanceFromPrevious = AimPoint.Distance(_previousTargetPosition, hitPosition);
        var angleFromPrevious = AimGeometry.AngleDegreesBetween(_previousTargetPosition, hitPosition);

        var next = _spawner.SpawnNextTarget(hitPosition, _layout, _settings, _randomSource);
        _previousTargetPosition = hitPosition;
        SetCurrentTarget(next.Target);

        return new TargetHit(hitPosition, distanceFromPrevious, angleFromPrevious);
    }

    private void SetCurrentTarget(TargetSnapshot target)
    {
        _currentTarget = target;
        _activeTargets = [target];
    }
}
