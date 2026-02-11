using Avalonia;
using EasySave.Bootstrap;
using EasySave.Presentation.Cli;

namespace EasySave;

internal static class Program
{
    internal static AppRuntime? Runtime { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            using var cliRuntime = AppRuntime.Create(interactiveProgress: false);
            Environment.ExitCode = CommandController.Run(
                args,
                cliRuntime.JobService,
                cliRuntime.BackupService,
                cliRuntime.StateService,
                cliRuntime.Validator);
            return;
        }

        Runtime = AppRuntime.Create(interactiveProgress: true);
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        Runtime.Dispose();
        Runtime = null;
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
