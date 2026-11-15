namespace VrtAimTrainer.Core.Features.About;

public sealed record AboutLink(string Id, string Label, string Handle, string Url);

public sealed record AboutCredit(string Name, string Role, string License);

/// <summary>
/// The single source for everything the About page shows that is not the app version
/// (the version comes from Version.props through the assembly).
/// </summary>
public static class AppAbout
{
    public const string AppName = "VrtAimTrainer";

    public const string Tagline = "A Windows aim trainer for measuring and improving reaction and aim.";

    public const string Summary =
        "Pressure-driven reactive sheets and grid drills, raw mouse input, and local session analytics.";

    public const string AuthorName = "Lukáš Krejčí";

    /// <summary>Fallback for fonts that cannot draw diacritics.</summary>
    public const string AuthorNameAscii = "Lukas Krejci";

    public const string FeedbackInvitation = "Feedback is welcome: what works, what does not, what is missing.";

    public const string LicenseName = "PolyForm Noncommercial 1.0.0";

    public static IReadOnlyList<AboutLink> Links { get; } =
    [
        new("instagram", "Instagram", "@lukas9krejci", "https://www.instagram.com/lukas9krejci/"),
        new("github", "GitHub", "lukr-99", "https://github.com/lukr-99"),
        new("issues", "Issues", "lukr-99/VrtAimTrainer", "https://github.com/lukr-99/VrtAimTrainer/issues"),
    ];

    public static IReadOnlyList<AboutCredit> Credits { get; } =
    [
        new("VrtAimTrainer", "The app itself", LicenseName),
        new("Raylib-cs 7.0.2", "C# bindings for raylib, used for rendering and input", "zlib"),
    ];
}
