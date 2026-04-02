namespace VrtAimTrainer.Desktop.Features.App;

public sealed record MouseSettings(
    float Sensitivity = 1f,
    float HorizontalScale = 1f,
    float VerticalScale = 1f,
    bool InvertVertical = false)
{
    public static MouseSettings Default { get; } = new();

    public float EffectiveHorizontalSensitivity => Sensitivity * HorizontalScale;

    public float EffectiveVerticalSensitivity => Sensitivity * VerticalScale * (InvertVertical ? -1f : 1f);

    public MouseSettings Normalize()
    {
        return this with
        {
            Sensitivity = Math.Clamp(Sensitivity, 0.20f, 3f),
            HorizontalScale = Math.Clamp(HorizontalScale, 0.50f, 2f),
            VerticalScale = Math.Clamp(VerticalScale, 0.50f, 2f),
        };
    }
}
