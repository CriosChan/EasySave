using EasySave.Core.Contracts;
using EasySave.Core.Models;
using EasySave.Data.Configuration;
using EasySave.Data.Logging;
using EasySave.Data.Persistence;
using EasySave.Platform.IO;
using EasySave.Platform.Localization;
using EasySave.Services;

namespace EasySave.Composition;

internal static class AppServicesFactory
{
    public static AppServices CreateForCli()
    {
        return CreateCore(new NullProgressReporter(), null);
    }

    public static AppServices CreateForUi()
    {
        var progressReporter = new UiProgressReporter();
        return CreateCore(progressReporter, progressReporter);
    }

    private static AppServices CreateCore(IProgressReporter progressReporter, UiProgressReporter? uiProgressReporter)
    {
        var cfg = ApplicationConfiguration.Load();

        var configDir = DataPathResolver.ResolveDirectory(cfg.JobConfigPath, "config");
        var logDir = DataPathResolver.ResolveDirectory(cfg.LogPath, "log");

        Directory.CreateDirectory(configDir);
        Directory.CreateDirectory(logDir);

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

        state.Initialize(jobService.GetAll());

        return new AppServices
        {
            JobService = jobService,
            BackupService = backupService,
            StateService = state,
            JobValidator = validator,
            StateSynchronizer = stateSynchronizer,
            UserPreferences = preferences,
            LocalizationApplier = localization,
            PathService = paths,
            UiProgressReporter = uiProgressReporter
        };
    }
}
