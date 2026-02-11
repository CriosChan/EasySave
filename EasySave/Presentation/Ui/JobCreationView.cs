using EasySave.Application.Abstractions;
using EasySave.Domain.Models;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
///     View for creating a backup job.
/// </summary>
internal sealed class JobCreationView
{
    private readonly IConsole _console;
    private readonly JobRepositoryErrorTranslator _errorTranslator;
    private readonly ConsolePrompter _prompter;
    private readonly IJobService _jobService;
    private readonly IStateSynchronizer _stateSync;

    /// <summary>
    ///     Builds the creation view.
    /// </summary>
    /// <param name="console">Target console.</param>
    /// <param name="jobService">Job service.</param>
    /// <param name="stateSync">State synchronizer.</param>
    /// <param name="prompter">Input prompter.</param>
    /// <param name="errorTranslator">Error translator.</param>
    public JobCreationView(
        IConsole console,
        IJobService jobService,
        IStateSynchronizer stateSync,
        ConsolePrompter prompter,
        JobRepositoryErrorTranslator errorTranslator)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _stateSync = stateSync ?? throw new ArgumentNullException(nameof(stateSync));
        _prompter = prompter ?? throw new ArgumentNullException(nameof(prompter));
        _errorTranslator = errorTranslator ?? throw new ArgumentNullException(nameof(errorTranslator));
    }

    /// <summary>
    ///     Displays the creation screen and saves the job.
    /// </summary>
    public void Show()
    {
        _console.Clear();
        _console.WriteLine(Resources.UserInterface.Add_Header);
        _console.WriteLine(string.Empty);

        var name = _prompter.ReadNonEmpty(Resources.UserInterface.Add_PromptName);
        var source = _prompter.ReadExistingDirectory(Resources.UserInterface.Add_PromptSource,
            Resources.UserInterface.Path_SourceNotFound);
        var target = _prompter.ReadExistingDirectory(Resources.UserInterface.Add_PromptTarget,
            Resources.UserInterface.Path_TargetNotFound);

        var type = _prompter.ReadBackupType(
            Resources.UserInterface.Add_PromptType,
            Resources.UserInterface.Add_TypeOptions,
            Resources.UserInterface.Common_InvalidInput);

        var newJob = new BackupJob(name, source, target, type);
        var (ok, error) = _jobService.AddJob(newJob);

        if (!ok)
        {
            _console.WriteLine(string.Empty);
            _console.WriteLine(Resources.UserInterface.Add_Failed);
            _console.WriteLine(_errorTranslator.Translate(error));
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            return;
        }

        _stateSync.Refresh();

        _console.WriteLine(string.Empty);
        _console.WriteLine(Resources.UserInterface.Add_Success);
        _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
    }
}
