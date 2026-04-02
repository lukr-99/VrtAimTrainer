using VrtAimTrainer.Core.Features.Analytics;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed record AimSessionStateSnapshot(
    SessionPhase Phase,
    AimSessionSettings Settings,
    AimPlayfieldLayout Layout,
    AimPoint CursorPosition,
    IReadOnlyList<TargetSnapshot> Targets,
    double Health,
    double Score,
    double ElapsedRunSeconds,
    int HitCount,
    double? LastReactionMs,
    double AverageReactionMs,
    double AverageHitsPerSecond,
    bool SettingsChangedDuringRun,
    SessionResult? LastCompletedResult,
    bool IsStartCountdownActive = false,
    int? StartCountdownValue = null,
    double StartCountdownRemainingSeconds = 0d)
{
    public TargetSnapshot Target => Targets[0];

    public static AimSessionStateSnapshot From(AimSessionState state)
    {
        return new AimSessionStateSnapshot(
            state.Phase,
            state.Settings,
            state.Layout,
            state.CursorPosition,
            state.Targets.ToArray(),
            state.Health,
            state.Score,
            state.ElapsedRunSeconds,
            state.HitCount,
            state.LastReactionMs,
            state.AverageReactionMs,
            state.AverageHitsPerSecond,
            state.SettingsChangedDuringRun,
            state.LastCompletedResult);
    }
}
