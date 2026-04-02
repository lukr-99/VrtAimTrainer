using System.Reflection;

namespace VrtAimTrainer.Desktop.Features.App;

internal static class AppVersionProvider
{
    public static string DisplayVersion { get; } = GetDisplayVersion();

    private static string GetDisplayVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return informationalVersion.Split('+')[0];
        }

        var version = assembly.GetName().Version;
        return version is null ? "dev" : version.ToString(3);
    }
}
