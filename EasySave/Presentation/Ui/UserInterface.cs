using EasySave.Application.Abstractions;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
/// Public entry-point for the interactive console UI.
/// </summary>
public static class UserInterface
{
    private static MainMenuController? _menu;

    /// <summary>
    /// Initializes UI components with the required services.
    /// </summary>
    /// <param name="repository">Job repository.</param>
    /// <param name="backupService">Backup service.</param>
    /// <param name="stateService">State service.</param>
    /// <param name="stateSynchronizer">State synchronizer.</param>
    /// <param name="paths">Path service.</param>
    public static void Initialize(
        IJobRepository repository,
        IBackupService backupService,
        IStateService stateService,
        IStateSynchronizer stateSynchronizer,
        IPathService paths)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));
        if (backupService == null) throw new ArgumentNullException(nameof(backupService));
        if (stateService == null) throw new ArgumentNullException(nameof(stateService));
        if (stateSynchronizer == null) throw new ArgumentNullException(nameof(stateSynchronizer));
        if (paths == null) throw new ArgumentNullException(nameof(paths));

        IConsole console = new SystemConsole();
        ConsolePrompter prompter = new ConsolePrompter(console, paths);
        JobRepositoryErrorTranslator errorTranslator = new JobRepositoryErrorTranslator();

        JobListView listView = new JobListView(console, repository, prompter);
        JobCreationView creationView = new JobCreationView(console, repository, stateSynchronizer, prompter, errorTranslator);
        JobRemovalView removalView = new JobRemovalView(console, repository, stateSynchronizer, prompter);
        JobLaunchView launchView = new JobLaunchView(console, repository, backupService, stateService, prompter, paths);

        _menu = new MainMenuController(console, listView, creationView, removalView, launchView);
    }

    /// <summary>
    /// Show the main menu to the user.
    /// </summary>
    public static void ShowMenu()
    {
        EnsureInitialized();
        _menu!.Show();
    }

    /// <summary>
    /// Ensures the UI has been initialized.
    /// </summary>
    private static void EnsureInitialized()
    {
        if (_menu == null)
            throw new InvalidOperationException("UserInterface.Initialize must be called before ShowMenu.");
    }
}
