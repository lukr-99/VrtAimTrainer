using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed record MouseSettings(
    float Sensitivity = 0.65f,
    float HorizontalScale = 1f,
    float VerticalScale = 1f,
    bool InvertVertical = false,
    AimResponseMode Mode = AimResponseMode.Linear,
    int MouseDpi = 800,
    float CentimetersPer360 = 30f,
    float HorizontalFovDegrees = 103f,
    float AspectWidth = 16f,
    float AspectHeight = 9f,
    float MonitorDistanceCoefficient = 1.33f)
{
    public static MouseSettings Default { get; } = new();

    // Linear-mode gains (logical units per raw count = these × RelativeAimMath base gain).
    public float EffectiveHorizontalSensitivity => Sensitivity * HorizontalScale;

    public float EffectiveVerticalSensitivity => Sensitivity * VerticalScale * (InvertVertical ? -1f : 1f);

    // Projected-mode per-axis multipliers on the cm/360-derived angular rate (Sensitivity is not
    // applied here, so the cm/360 figure stays physically accurate; 1.0 scales give a true 1:1 feel).
    public float VerticalSign => InvertVertical ? -1f : 1f;

    public double Aspect => AspectHeight <= 0f ? 1d : (double)AspectWidth / AspectHeight;

    public MouseSettings Normalize()
    {
        return this with
        {
            Sensitivity = Math.Clamp(Sensitivity, 0.20f, 3f),
            HorizontalScale = Math.Clamp(HorizontalScale, 0.50f, 2f),
            VerticalScale = Math.Clamp(VerticalScale, 0.50f, 2f),
            MouseDpi = MouseDpi <= 0 ? 800 : Math.Clamp(MouseDpi, 100, 32000),
            CentimetersPer360 = CentimetersPer360 <= 0f ? 30f : Math.Clamp(CentimetersPer360, 1f, 200f),
            HorizontalFovDegrees = HorizontalFovDegrees <= 0f ? 103f : Math.Clamp(HorizontalFovDegrees, 60f, 150f),
            AspectWidth = AspectWidth <= 0f ? 16f : AspectWidth,
            AspectHeight = AspectHeight <= 0f ? 9f : AspectHeight,
            MonitorDistanceCoefficient = MonitorDistanceCoefficient <= 0f ? 1.33f : Math.Clamp(MonitorDistanceCoefficient, 0.10f, 2f),
        };
    }
}
