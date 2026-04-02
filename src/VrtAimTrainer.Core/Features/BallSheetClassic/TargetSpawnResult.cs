using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.BallSheetClassic;

public readonly record struct TargetSpawnResult(TargetSnapshot Target, double DistanceFromPreviousTarget);
