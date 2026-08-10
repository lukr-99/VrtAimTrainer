namespace VrtAimTrainer.Core.Features.Gameplay;

/// <summary>
/// Encapsulates everything that differs between game modes: where targets appear, how a hit is
/// detected, and what replaces a hit target. The <see cref="AimSessionEngine"/> owns the shared
/// session loop (timing, health, scoring, analytics) and delegates all target concerns here.
/// <para>
/// A new game mode is therefore just a new <see cref="ITargetProvider"/> — no new controller.
/// </para>
/// </summary>
public interface ITargetProvider
{
    /// <summary>The currently live targets. Always contains at least one target.</summary>
    IReadOnlyList<TargetSnapshot> ActiveTargets { get; }

    /// <summary>
    /// One-time setup at construction. Must build any layout-dependent structures and populate
    /// <see cref="ActiveTargets"/> deterministically (no randomness) so the engine can build state;
    /// <see cref="Reset"/> performs the first randomized seed.
    /// </summary>
    void Initialize(AimPlayfieldLayout layout, AimSessionSettings settings);

    /// <summary>Rebuilds target placement for a new layout and/or settings, keeping the run coherent.</summary>
    void Reconfigure(AimPlayfieldLayout layout, AimSessionSettings settings);

    /// <summary>Seeds the pre-game target set (shown before the run starts).</summary>
    void Reset();

    /// <summary>Seeds the target set for the start of a run.</summary>
    void BeginRun();

    /// <summary>
    /// If <paramref name="cursor"/> is over a live target, advances the target set and returns the hit
    /// (with distance/angle from the previous target for analytics); otherwise returns <c>null</c>.
    /// </summary>
    TargetHit? TryRegisterHit(AimPoint cursor);
}
