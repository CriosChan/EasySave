using EasySave.Application.Abstractions;
using EasySave.Application.Services;
using EasySave.Domain.Models;
using EasySave.Infrastructure.Configuration;
using EasySave.Infrastructure.IO;
using EasySave.Infrastructure.Lang;
using EasySave.Infrastructure.Logging;
using EasySave.Infrastructure.Persistence;
using EasySave.Infrastructure.Security;
using EasySave.Infrastructure.System;

namespace EasySave.Bootstrap;

/// <summary>
///     Composition root shared by CLI and Avalonia UI.
/// </summary>
public sealed class AppRuntime : IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    private AppRuntime(
        IJobService jobService,
        IJobValidator validator,
        IStateService stateService,
        IStateSynchronizer stateSynchronizer,
        IBackupService backupService,
        IBackupRuntimeController backupRuntime,
        IUserPreferences preferences,
        ILocalizationApplier localization,
        IGeneralSettingsStore generalSettings)
    {
        JobService = jobService;
        Validator = validator;
        StateService = stateService;
        StateSynchronizer = stateSynchronizer;
        BackupService = backupService;
        BackupRuntime = backupRuntime;
        Preferences = preferences;
        Localization = localization;
        GeneralSettings = generalSettings;
    }

    public IJobService JobService { get; }
    public IJobValidator Validator { get; }
    public IStateService StateService { get; }
    public IStateSynchronizer StateSynchronizer { get; }
    public IBackupService BackupService { get; }
    public IBackupRuntimeController BackupRuntime { get; }
    public IUserPreferences Preferences { get; }
    public ILocalizationApplier Localization { get; }
    public IGeneralSettingsStore GeneralSettings { get; }

    public static AppRuntime Create(bool interactiveProgress)
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
        IJobValidator validator = new JobValidator(paths);
        IStateSynchronizer stateSynchronizer = new StateSynchronizer(repository, state);

        IUserPreferences preferences = new UserPreferencesStore(configDir, cfg.Localization, "json");
        ILocalizationApplier localization = new LocalizationApplier();
        localization.Apply(preferences.Localization);

        IGeneralSettingsStore settings = new GeneralSettingsStore(configDir);
        var logWriter = BuildLogWriter(logDir, preferences, settings, out var managedDisposables);

        var selector = new BackupFileSelector(paths);
        var directoryPreparer = new BackupDirectoryPreparer(logWriter, paths);
        var fileCopier = new FileCopier();
        IProgressReporter progress = interactiveProgress ? new NullProgressReporter() : new NullProgressReporter();
        IBusinessSoftwareDetector businessDetector = new BusinessSoftwareDetector();
        IFileEncryptionService encryption = new CryptoSoftEncryptionService(settings);

        var backup = new BackupService(
            logWriter,
            state,
            paths,
            selector,
            directoryPreparer,
            fileCopier,
            validator,
            progress,
            settings,
            businessDetector,
            encryption);

        state.Initialize(jobService.GetAll());

        var runtime = new AppRuntime(
            jobService,
            validator,
            state,
            stateSynchronizer,
            backup,
            backup,
            preferences,
            localization,
            settings);

        runtime._disposables.AddRange(managedDisposables);
        runtime._disposables.Add(backup);
        return runtime;
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            try
            {
                disposable.Dispose();
            }
            catch
            {
                // Best effort.
            }
        }
    }

    private static ILogWriter<LogEntry> BuildLogWriter(
        string logDirectory,
        IUserPreferences preferences,
        IGeneralSettingsStore settings,
        out List<IDisposable> disposables)
    {
        disposables = [];

        ILogWriter<LogEntry> jsonWriter = new JsonLogWriter<LogEntry>(logDirectory);
        ILogWriter<LogEntry> xmlWriter = new XmlLogWriter<LogEntry>(logDirectory);
        ILogWriter<LogEntry> localWriter = new ConfigurableLogWriter<LogEntry>(preferences, jsonWriter, xmlWriter);

        var adaptive = new AdaptiveLogWriter<LogEntry>(
            localWriter,
            settings,
            endpoint => new HttpLogWriter<LogEntry>(endpoint));
        disposables.Add(adaptive);
        return adaptive;
    }
}
