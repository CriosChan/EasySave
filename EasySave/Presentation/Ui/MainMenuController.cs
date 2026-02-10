using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
///     Builds and displays the main interactive menu.
/// </summary>
internal sealed class MainMenuController
{
    private readonly IConsole _console;
    private readonly JobCreationView _jobCreation;
    private readonly JobLaunchView _jobLaunch;
    private readonly JobListView _jobList;
    private readonly JobRemovalView _jobRemoval;

    /// <summary>
    ///     Builds the main menu controller.
    /// </summary>
    /// <param name="console">Target console.</param>
    /// <param name="jobList">Job list view.</param>
    /// <param name="jobCreation">Job creation view.</param>
    /// <param name="jobRemoval">Job removal view.</param>
    /// <param name="jobLaunch">Job launch view.</param>
    public MainMenuController(
        IConsole console,
        JobListView jobList,
        JobCreationView jobCreation,
        JobRemovalView jobRemoval,
        JobLaunchView jobLaunch)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _jobList = jobList ?? throw new ArgumentNullException(nameof(jobList));
        _jobCreation = jobCreation ?? throw new ArgumentNullException(nameof(jobCreation));
        _jobRemoval = jobRemoval ?? throw new ArgumentNullException(nameof(jobRemoval));
        _jobLaunch = jobLaunch ?? throw new ArgumentNullException(nameof(jobLaunch));
    }

    /// <summary>
    ///     Displays the main menu and handles navigation.
    /// </summary>
    public void Show()
    {
        ListWidget.ShowList(
        [
            new Option(Resources.UserInterface.Menu_ListJobs, _jobList.Show),
            new Option(Resources.UserInterface.Menu_AddBackup, _jobCreation.Show),
            new Option(Resources.UserInterface.Menu_RemoveBackup, _jobRemoval.Show),
            new Option(Resources.UserInterface.Menu_LaunchBackupJob, _jobLaunch.Show),
            new Option(Resources.UserInterface.Menu_Lang, new LanguageView(_console).Show),
            new Option(Resources.UserInterface.Menu_Log, new LogTypeView(_console).Show),
            new Option(Resources.UserInterface.Menu_Quit, () => Environment.Exit(0))
        ], _console, Resources.UserInterface.Menu_Title_Main);
    }
}