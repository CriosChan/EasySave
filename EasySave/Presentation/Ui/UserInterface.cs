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
    /// Initialise les composants de l'UI avec les services necessaires.
    /// </summary>
    /// <param name="repository">Depot des jobs.</param>
    /// <param name="backupService">Service de sauvegarde.</param>
    /// <param name="stateService">Service d'etat.</param>
    /// <param name="stateSynchronizer">Synchroniseur d'etat.</param>
    /// <param name="paths">Service de chemins.</param>
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
    /// Verifie que l'UI a bien ete initialisee.
    /// </summary>
    private static void EnsureInitialized()
    {
        if (_menu == null)
            throw new InvalidOperationException("UserInterface.Initialize must be called before ShowMenu.");
    }
}
