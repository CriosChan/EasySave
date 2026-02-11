using EasySave.Application.Abstractions;
using EasySave.Application.Services;
using EasySave.Domain.Models;
using EasySave.Infrastructure.Configuration;
using EasySave.Infrastructure.IO;
using EasySave.Infrastructure.Lang;
using EasySave.Infrastructure.Logging;
using EasySave.Infrastructure.Persistence;
using EasySave.Presentation.Cli;
using EasySave.Presentation.Ui;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Bootstrap;

/// <summary>
///     Composition root of the application. It wires configuration, services and UI/CLI.
/// </summary>
internal sealed class EasySaveApplication : IApplication
{
    /// <summary>
    ///     Application entry point: loads configuration, builds services, and starts CLI or UI mode.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Execution exit code.</returns>
    public int Run(string[] args)
    {
        var cfg = ApplicationConfiguration.Load();

        // Resolve data directories to OS-appropriate locations.
        var configDir = DataPathResolver.ResolveDirectory(cfg.JobConfigPath, "config");
        var logDir = DataPathResolver.ResolveDirectory(cfg.LogPath, "log");

        Directory.CreateDirectory(configDir);
        Directory.CreateDirectory(logDir);

        // Core services.
        IPathService paths = new PathService();
        IJobRepository repository = new JobRepository(configDir);
        IStateService state = new StateFileService(configDir);
        IJobService jobService = new JobService(repository);

        IUserPreferences preferences = new UserPreferencesStore(configDir, cfg.Localization, "json");
        ILocalizationApplier localization = new LocalizationApplier();
        localization.Apply(preferences.Localization);

        ILogWriter<LogEntry> jsonWriter = new JsonLogWriter<LogEntry>(logDir);
        ILogWriter<LogEntry> xmlWriter = new XmlLogWriter<LogEntry>(logDir);
        ILogWriter<LogEntry> logWriter = new ConfigurableLogWriter<LogEntry>(preferences, jsonWriter, xmlWriter);

        var fileSelector = new BackupFileSelector(paths);
        var directoryPreparer = new BackupDirectoryPreparer(logWriter, paths);
        var fileCopier = new FileCopier();
        IJobValidator validator = new JobValidator(paths);

        IProgressReporter progressReporter = new NullProgressReporter();
        IConsole? console = null;
        if (args.Length == 0)
        {
            console = new SystemConsole();
            progressReporter = new ConsoleProgressReporter(console);
        }

        IBackupService backupService = new BackupService(
            logWriter,
            state,
            paths,
            fileSelector,
            directoryPreparer,
            fileCopier,
            validator,
            progressReporter);

        IStateSynchronizer stateSynchronizer = new StateSynchronizer(repository, state);

        // Initialize state.json with the configured jobs.
        state.Initialize(jobService.GetAll());

        // Command-line mode (automatic execution) or interactive mode.
        if (args.Length > 0)
            return CommandController.Run(args, jobService, backupService, state, validator);

        // Build interactive UI.
        console ??= new SystemConsole();
        var prompter = new ConsolePrompter(console, paths);
        var errorTranslator = new JobRepositoryErrorTranslator();
        var navigator = new MenuNavigator();

        var listView = new JobListView(console, jobService, prompter);
        var creationView = new JobCreationView(console, jobService, stateSynchronizer, prompter, errorTranslator);
        var removalView = new JobRemovalView(console, jobService, stateSynchronizer, prompter, navigator);
        var launchView = new JobLaunchView(console, jobService, backupService, prompter, validator, navigator);
        var languageView = new LanguageView(console, preferences, localization, navigator);
        var logTypeView = new LogTypeView(console, preferences, navigator);

        var menu = new MainMenuController(
            console,
            listView,
            creationView,
            removalView,
            launchView,
            languageView,
            logTypeView);

        navigator.Attach(menu);
        navigator.ShowMainMenu();
        return 0;
    }
}
