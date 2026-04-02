namespace VrtAimTrainer.Desktop.Features.Gamemodes;

internal static class GamemodePluginPathProvider
{
    public static string PrimaryDirectory =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VrtAimTrainer",
            "Plugins");

    public static IReadOnlyList<string> GetSearchDirectories()
    {
        var directories = new[]
        {
            PrimaryDirectory,
            Path.Combine(AppContext.BaseDirectory, "Plugins"),
        };

        return directories
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
