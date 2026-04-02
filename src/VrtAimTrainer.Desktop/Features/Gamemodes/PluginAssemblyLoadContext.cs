using System.Reflection;
using System.Runtime.Loader;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.Gamemodes;

internal sealed class PluginAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginAssemblyLoadContext(string mainAssemblyPath)
        : base(isCollectible: false)
    {
        _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var sharedAssembly = ResolveSharedAssembly(assemblyName);
        if (sharedAssembly is not null)
        {
            return sharedAssembly;
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath is null ? null : LoadFromAssemblyPath(assemblyPath);
    }

    private static Assembly? ResolveSharedAssembly(AssemblyName assemblyName)
    {
        var sharedAssemblyNames = new[]
        {
            typeof(IAimGamemodePlugin).Assembly.GetName().Name,
        };

        if (!sharedAssemblyNames.Contains(assemblyName.Name, StringComparer.Ordinal))
        {
            return null;
        }

        return AssemblyLoadContext.Default.Assemblies
            .FirstOrDefault(assembly => string.Equals(
                assembly.GetName().Name,
                assemblyName.Name,
                StringComparison.Ordinal));
    }
}
