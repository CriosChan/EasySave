using EasyLog;
using EasySave.Application.Abstractions;
using EasySave.Application.Services;
using EasySave.Domain.Models;
using EasySave.Infrastructure.Configuration;
using EasySave.Infrastructure.IO;
using EasySave.Infrastructure.Lang;
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
    /// Application entry point: loads configuration, builds services, and starts CLI or UI mode.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Execution exit code.</returns>
    public int Run(string[] args)
    {
        // Load configuration (paths, localization).
        ApplicationConfiguration.Load();
        ApplicationConfiguration cfg = ApplicationConfiguration.Instance;

        // Apply localization early so menus/prompts pick the right resource.
        LangUtil.TryApplyCulture(cfg.Localization);

        // Resolve data directories to OS-appropriate locations.
        string configDir = DataPathResolver.ResolveDirectory(cfg.JobConfigPath, "config");
        string logDir = DataPathResolver.ResolveDirectory(cfg.LogPath, "log");

        Directory.CreateDirectory(configDir);
        Directory.CreateDirectory(logDir);

        // Create services.
        IPathService paths = new PathService();
        IJobRepository repository = new JobRepository(configDir);
        IStateService state = new StateFileService(configDir);

        
        AbstractLogger<LogEntry> logWriter;
        if (cfg.LogType == "xml")
        {
            logWriter = new XmlLogger<LogEntry>(logDir);
        }
        else
        {
            logWriter = new JsonLogger<LogEntry>(logDir);
        }
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
}
