using VrtAimTrainer.Desktop.Features.App;

namespace VrtAimTrainer.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var application = new TrainerApplication();
        application.Run();
    }
}
