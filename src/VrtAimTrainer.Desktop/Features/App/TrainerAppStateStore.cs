using System.Text.Json;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed class TrainerAppStateStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public TrainerAppStateStore(string filePath)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }

    public TrainerAppState? Load()
    {
        if (!File.Exists(FilePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<TrainerAppState>(json, SerializerOptions);
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public void Save(TrainerAppState state)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var json = JsonSerializer.Serialize(state, SerializerOptions);
        File.WriteAllText(FilePath, json);
    }
}
