using System.Text.Json;
using VrtAimTrainer.Core.Features.Analytics;

namespace VrtAimTrainer.Core.Features.History;

public sealed class JsonSessionHistoryStore : ISessionHistoryStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public JsonSessionHistoryStore(string storageDirectory)
    {
        StorageDirectory = storageDirectory;
    }

    public string StorageDirectory { get; }

    public void Save(SessionResult result)
    {
        Directory.CreateDirectory(StorageDirectory);

        var fileName = $"{result.CompletedAtUtc:yyyyMMddTHHmmssfff}_{result.SessionId:N}.json";
        var fullPath = Path.Combine(StorageDirectory, fileName);
        var json = JsonSerializer.Serialize(result, SerializerOptions);
        File.WriteAllText(fullPath, json);
    }

    public IReadOnlyList<SessionResult> LoadRecent(int take)
    {
        if (!Directory.Exists(StorageDirectory) || take <= 0)
        {
            return [];
        }

        var results = new List<SessionResult>();
        foreach (var fullPath in Directory
                     .EnumerateFiles(StorageDirectory, "*.json")
                     .OrderByDescending(Path.GetFileName)
                     .Take(take))
        {
            try
            {
                var json = File.ReadAllText(fullPath);
                var result = JsonSerializer.Deserialize<SessionResult>(json, SerializerOptions);
                if (result is not null)
                {
                    results.Add(result);
                }
            }
            catch (IOException)
            {
                continue;
            }
            catch (JsonException)
            {
                continue;
            }
        }

        return results;
    }
}
