using VrtAimTrainer.Core.Features.Analytics;

namespace VrtAimTrainer.Core.Features.Gameplay;

public sealed class AimSessionState
{
    public AimSessionState(AimSessionSettings settings, AimPlayfieldLayout layout, TargetSnapshot target)
    {
        Settings = settings;
        Layout = layout;
        Health = settings.StartingHealth;
        SetTarget(target);
    }

    public SessionPhase Phase { get; set; } = SessionPhase.PreGame;

    public AimSessionSettings Settings { get; set; }

    public AimPlayfieldLayout Layout { get; set; }

    public AimPoint CursorPosition { get; set; }

    public TargetSnapshot Target { get; private set; }

    public IReadOnlyList<TargetSnapshot> Targets { get; private set; } = [];

    public double Health { get; set; }

    public double Score { get; set; }

    public double ElapsedRunSeconds { get; set; }

    public int HitCount { get; set; }

    public double? LastReactionMs { get; set; }

    public double AverageReactionMs { get; set; }

    public double AverageHitsPerSecond { get; set; }

    public bool SettingsChangedDuringRun { get; set; }

    public SessionResult? LastCompletedResult { get; set; }

    public void SetTarget(TargetSnapshot target)
    {
        SetTargets([target]);
    }

    public void SetTargets(IReadOnlyList<TargetSnapshot> targets)
    {
        if (targets.Count == 0)
        {
            throw new ArgumentException("At least one target must be provided.", nameof(targets));
        }

        Targets = targets;
        Target = targets[0];
    }
}
