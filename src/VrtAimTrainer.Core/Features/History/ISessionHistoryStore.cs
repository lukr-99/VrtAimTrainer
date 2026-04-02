using VrtAimTrainer.Core.Features.Analytics;

namespace VrtAimTrainer.Core.Features.History;

public interface ISessionHistoryStore
{
    string StorageDirectory { get; }

    void Save(SessionResult result);

    IReadOnlyList<SessionResult> LoadRecent(int take);
}
