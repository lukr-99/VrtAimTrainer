using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Core.Features.GridShot;

public sealed class GridShotCellSelector
{
    public IReadOnlyList<int> CreateInitialSelection(int cellCount, int concurrentTargetCount, IRandomSource randomSource)
    {
        var selectedCount = Math.Clamp(concurrentTargetCount, 1, Math.Max(1, cellCount));
        var available = Enumerable.Range(0, cellCount).ToList();
        var selected = new List<int>(selectedCount);

        while (selected.Count < selectedCount && available.Count > 0)
        {
            var nextIndex = NextIndex(available.Count, randomSource);
            selected.Add(available[nextIndex]);
            available.RemoveAt(nextIndex);
        }

        return selected;
    }

    public int PickReplacementCellIndex(int cellCount, IReadOnlyCollection<int> activeCellIndices, int fallbackCellIndex, IRandomSource randomSource)
    {
        var available = new List<int>(cellCount);
        for (var cellIndex = 0; cellIndex < cellCount; cellIndex++)
        {
            if (!activeCellIndices.Contains(cellIndex) && cellIndex != fallbackCellIndex)
            {
                available.Add(cellIndex);
            }
        }

        if (available.Count == 0)
        {
            return fallbackCellIndex;
        }

        return available[NextIndex(available.Count, randomSource)];
    }

    private static int NextIndex(int maxExclusive, IRandomSource randomSource)
    {
        if (maxExclusive <= 1)
        {
            return 0;
        }

        var rawIndex = (int)Math.Floor(randomSource.NextDouble() * maxExclusive);
        return Math.Clamp(rawIndex, 0, maxExclusive - 1);
    }
}
