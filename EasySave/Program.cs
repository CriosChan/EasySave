using Avalonia;
using EasySave.Cli;
using EasySave.Composition;

namespace EasySave;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            var services = AppServicesFactory.CreateForCli();
            Environment.ExitCode = CommandController.Run(
                args,
                services.JobService,
                services.BackupService,
                services.StateService,
                services.JobValidator);
            return;
        }

        AppRuntime.Services = AppServicesFactory.CreateForUi();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
