namespace VrtAimTrainer.Core.Features.Gameplay;

public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random _random;

    public SystemRandomSource()
        : this(Random.Shared)
    {
    }

    public SystemRandomSource(Random random)
    {
        _random = random;
    }

    public double NextDouble()
    {
        return _random.NextDouble();
    }
}
