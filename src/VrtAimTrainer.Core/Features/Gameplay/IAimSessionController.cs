namespace VrtAimTrainer.Core.Features.Gameplay;

public interface IAimSessionController
{
    string GamemodeId { get; }

    string DisplayName { get; }

    AimSessionState State { get; }

    IReadOnlyList<HitSample> Samples { get; }

    void ResizeViewport(int viewportWidth, int viewportHeight);

    void UpdateSettings(AimSessionSettings settings);

    void Update(in AimFrameInput input);

    bool Start(double timestampSeconds);

    bool Finish(double timestampSeconds);

    void Reset();
}
