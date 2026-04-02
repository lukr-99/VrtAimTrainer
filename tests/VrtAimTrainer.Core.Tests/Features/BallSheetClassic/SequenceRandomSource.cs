using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Tests.Features.BallSheetClassic;

internal sealed class SequenceRandomSource : IRandomSource
{
    private readonly Queue<double> _values;

    public SequenceRandomSource(IEnumerable<double> values)
    {
        _values = new Queue<double>(values);
    }

    public double NextDouble()
    {
        if (_values.Count == 0)
        {
            throw new InvalidOperationException("No more deterministic random values were queued.");
        }

        return _values.Dequeue();
    }
}
