namespace VrtAimTrainer.Core.Features.Gameplay;

public readonly record struct TargetSnapshot(AimPoint Position, float Diameter)
{
    public float Radius => Diameter / 2f;
}
