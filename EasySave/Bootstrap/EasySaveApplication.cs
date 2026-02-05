using System.Globalization;
using EasySave.Application.Abstractions;
using EasySave.Application.Services;
using EasySave.Domain.Models;
using EasySave.Infrastructure.Configuration;
using EasySave.Infrastructure.IO;
using EasySave.Infrastructure.Logging;
using EasySave.Infrastructure.Persistence;
using EasySave.Presentation.Cli;
using EasySave.Presentation.Ui;

namespace EasySave.Bootstrap;

/// <summary>
/// Composition root of the application. It wires configuration, services and UI/CLI.
/// </summary>
internal sealed class EasySaveApplication : IApplication
{
    /// <summary>
    /// Point d'entree applicatif: charge la configuration, construit les services et demarre le mode CLI ou UI.
    /// </summary>
    /// <param name="args">Arguments de ligne de commande.</param>
    /// <returns>Code de sortie de l'execution.</returns>
    public int Run(string[] args)
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
        IPathService paths = new PathService();
        IJobRepository repository = new JobRepository(configDir);
        IStateService state = new StateFileService(configDir);

        ILogWriter<LogEntry> logWriter = new JsonLogWriter<LogEntry>(logDir);
        IBackupFileSelector fileSelector = new BackupFileSelector(paths);
        IBackupDirectoryPreparer directoryPreparer = new BackupDirectoryPreparer(logWriter, paths);
        IFileCopier fileCopier = new FileCopier();
        IBackupService backupService = new BackupService(logWriter, state, paths, fileSelector, directoryPreparer, fileCopier);
        IStateSynchronizer stateSynchronizer = new StateSynchronizer(repository, state);

        // Initialize state.json with the configured jobs.
        state.Initialize(repository.Load());

        // Command-line mode (automatic execution) or interactive mode.
        if (args.Length > 0)
            return CommandController.Run(args, repository, backupService, state, paths);

        UserInterface.Initialize(repository, backupService, state, stateSynchronizer, paths);
        UserInterface.ShowMenu();
        return 0;
    }

    /// <summary>
    /// Applique la culture UI/Thread si elle est valide.
    /// </summary>
    /// <param name="cultureName">Nom de culture (ex: fr-FR).</param>
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
