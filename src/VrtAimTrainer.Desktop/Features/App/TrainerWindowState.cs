using System.Numerics;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed class TrainerWindowState
{
    public string ActiveDisplayProfileId { get; set; } = "1080p";

    public bool CursorHidden { get; set; }

    public bool IsBorderlessFullscreen { get; set; }

    public int WindowedHeight { get; set; } = 1080;

    public Vector2 WindowedPosition { get; set; } = new(120f, 120f);

    public int WindowedWidth { get; set; } = 1920;
}
