using Raylib_cs;

namespace VrtAimTrainer.Desktop.Features.App;

internal static class WindowIconLoader
{
    public static void TrySetWindowIcon()
    {
        var iconPath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Branding",
            "VrtAimTrainerIcon.png");

        if (!File.Exists(iconPath))
        {
            return;
        }

        var iconImage = Raylib.LoadImage(iconPath);
        if (iconImage.Width <= 0 || iconImage.Height <= 0)
        {
            return;
        }

        try
        {
            Raylib.SetWindowIcon(iconImage);
        }
        finally
        {
            Raylib.UnloadImage(iconImage);
        }
    }
}
