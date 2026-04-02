using System.Reflection;
using VrtAimTrainer.Core.Features.BallSheetClassic;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.Gamemodes;

public sealed class AimGamemodeCatalog
{
    public AimGamemodeCatalog(
        IReadOnlyList<LoadedAimGamemode> gamemodes,
        IReadOnlyList<AimGamemodePluginLoadFailure> loadFailures,
        IReadOnlyList<string> searchDirectories)
    {
        Gamemodes = gamemodes;
        LoadFailures = loadFailures;
        SearchDirectories = searchDirectories;
    }

    public IReadOnlyList<LoadedAimGamemode> Gamemodes { get; }

    public IReadOnlyList<AimGamemodePluginLoadFailure> LoadFailures { get; }

    public IReadOnlyList<string> SearchDirectories { get; }

    public static AimGamemodeCatalog CreateDefault()
    {
        Directory.CreateDirectory(GamemodePluginPathProvider.PrimaryDirectory);

        var searchDirectories = GamemodePluginPathProvider.GetSearchDirectories();
        var loadFailures = new List<AimGamemodePluginLoadFailure>();
        var gamemodes = new List<LoadedAimGamemode>();
        var registeredIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        RegisterPlugin(
            plugin: new BallSheetClassicGamemodePlugin(),
            sourcePath: "Built-in",
            isBuiltIn: true,
            gamemodes,
            loadFailures,
            registeredIds);

        foreach (var searchDirectory in searchDirectories.Where(Directory.Exists))
        {
            foreach (var assemblyPath in Directory.EnumerateFiles(searchDirectory, "*.dll", SearchOption.AllDirectories))
            {
                LoadExternalAssembly(
                    assemblyPath,
                    gamemodes,
                    loadFailures,
                    registeredIds);
            }
        }

        return new AimGamemodeCatalog(
            gamemodes.OrderBy(gamemode => gamemode.DisplayName, StringComparer.OrdinalIgnoreCase).ToArray(),
            loadFailures,
            searchDirectories);
    }

    public LoadedAimGamemode GetDefault()
    {
        return Gamemodes.First(gamemode => string.Equals(gamemode.Id, "ballsheet-classic", StringComparison.OrdinalIgnoreCase));
    }

    private static void LoadExternalAssembly(
        string assemblyPath,
        ICollection<LoadedAimGamemode> gamemodes,
        ICollection<AimGamemodePluginLoadFailure> loadFailures,
        ISet<string> registeredIds)
    {
        try
        {
            var loadContext = new PluginAssemblyLoadContext(assemblyPath);
            var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
            var pluginTypes = assembly
                .GetTypes()
                .Where(type =>
                    !type.IsAbstract &&
                    typeof(IAimGamemodePlugin).IsAssignableFrom(type) &&
                    type.GetConstructor(Type.EmptyTypes) is not null)
                .ToArray();

            if (pluginTypes.Length == 0)
            {
                return;
            }

            foreach (var pluginType in pluginTypes)
            {
                if (Activator.CreateInstance(pluginType) is not IAimGamemodePlugin plugin)
                {
                    loadFailures.Add(new AimGamemodePluginLoadFailure(
                        assemblyPath,
                        $"Could not instantiate plugin type '{pluginType.FullName}'."));
                    continue;
                }

                RegisterPlugin(plugin, assemblyPath, false, gamemodes, loadFailures, registeredIds);
            }
        }
        catch (ReflectionTypeLoadException exception)
        {
            var loaderErrors = string.Join(
                " | ",
                exception.LoaderExceptions
                    .Where(loaderException => loaderException is not null)
                    .Select(loaderException => loaderException!.Message));

            loadFailures.Add(new AimGamemodePluginLoadFailure(
                assemblyPath,
                string.IsNullOrWhiteSpace(loaderErrors) ? exception.Message : loaderErrors));
        }
        catch (Exception exception)
        {
            loadFailures.Add(new AimGamemodePluginLoadFailure(assemblyPath, exception.Message));
        }
    }

    private static void RegisterPlugin(
        IAimGamemodePlugin plugin,
        string sourcePath,
        bool isBuiltIn,
        ICollection<LoadedAimGamemode> gamemodes,
        ICollection<AimGamemodePluginLoadFailure> loadFailures,
        ISet<string> registeredIds)
    {
        foreach (var gamemode in plugin.CreateGamemodes())
        {
            if (!registeredIds.Add(gamemode.Id))
            {
                loadFailures.Add(new AimGamemodePluginLoadFailure(
                    sourcePath,
                    $"Skipped duplicate gamemode id '{gamemode.Id}' from plugin '{plugin.Metadata.DisplayName}'."));
                continue;
            }

            gamemodes.Add(new LoadedAimGamemode(gamemode, plugin.Metadata, sourcePath, isBuiltIn));
        }
    }
}
