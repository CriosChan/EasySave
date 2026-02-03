using System.Globalization;
using EasySave.Controller;
using EasySave.Services;
using EasySave.Utils;
using EasySave.View;

namespace EasySave;

internal static class Program
{
    private static void Main(string[] args)
    {
        // Load configuration (paths, localization).
        ApplicationConfiguration.Load();
        ApplicationConfiguration cfg = ApplicationConfiguration.Instance;

        // Apply localization early so menus/prompts pick the right resource.
        TryApplyCulture(cfg.Localization);

        // Resolve data directories to OS-appropriate locations.
        string configDir = DataPathResolver.ResolveDirectory(cfg.JobConfigPath, "config");
        string logDir = DataPathResolver.ResolveDirectory(cfg.LogPath, "log");

        Directory.CreateDirectory(configDir);
        Directory.CreateDirectory(logDir);

        // Create services.
        JobRepository repository = new JobRepository(configDir);
        StateFileService state = new StateFileService(configDir);
        BackupService backupService = new BackupService(logDir, state);

        // Initialize state.json with the configured jobs.
        state.Initialize(repository.Load());

        // Command-line mode (automatic execution) or interactive mode.
        if (args.Length > 0)
        {
            int code = CommandController.Run(args, repository, backupService, state);
            Environment.ExitCode = code;
            return;
        }

        UserInterface.Initialize(repository, backupService, state);
        UserInterface.ShowMenu();
    }

    private static void TryApplyCulture(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
            return;

        try
        {
            CultureInfo culture = new CultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
        catch
        {
            // If localization is invalid, keep the default system culture.
        }
    }
}