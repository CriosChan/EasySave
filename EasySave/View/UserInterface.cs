using EasySave.Services;
using EasySave.View.Console;

namespace EasySave.View;

/// <summary>
/// Public entry-point for the interactive console UI.
/// </summary>
public static class UserInterface
{
    private static MainMenuController? _menu;

    public static void Initialize(JobRepository repository, BackupService backupService, StateFileService stateService)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));
        if (backupService == null) throw new ArgumentNullException(nameof(backupService));
        if (stateService == null) throw new ArgumentNullException(nameof(stateService));

        IConsole console = new SystemConsole();
        ConsolePrompter prompter = new ConsolePrompter(console);

        StateFileSynchronizer stateSync = new StateFileSynchronizer(repository, stateService);
        JobRepositoryErrorTranslator errorTranslator = new JobRepositoryErrorTranslator();

        JobListView listView = new JobListView(console, repository, prompter);
        JobCreationView creationView = new JobCreationView(console, repository, stateSync, prompter, errorTranslator);
        JobRemovalView removalView = new JobRemovalView(console, repository, stateSync, prompter);
        JobLaunchView launchView = new JobLaunchView(console, repository, backupService, stateService, prompter);

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

    private static void EnsureInitialized()
    {
        if (_menu == null)
            throw new InvalidOperationException("UserInterface.Initialize must be called before ShowMenu.");
    }
}
