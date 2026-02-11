using EasySave.Application.Abstractions;
using EasySave.Application.Models;
using EasySave.Domain.Models;
using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
///     View for launching a backup job.
/// </summary>
internal sealed class JobLaunchView
{
    private readonly IBackupService _backupService;
    private readonly IConsole _console;
    private readonly IJobValidator _validator;
    private readonly IMenuNavigator _navigator;
    private readonly ConsolePrompter _prompter;
    private readonly IJobService _jobService;

    /// <summary>
    ///     Builds the launch view.
    /// </summary>
    /// <param name="console">Target console.</param>
    /// <param name="jobService">Job service.</param>
    /// <param name="backupService">Backup service.</param>
    /// <param name="prompter">Input prompter.</param>
    /// <param name="validator">Job validator.</param>
    /// <param name="navigator">Menu navigator.</param>
    public JobLaunchView(
        IConsole console,
        IJobService jobService,
        IBackupService backupService,
        ConsolePrompter prompter,
        IJobValidator validator,
        IMenuNavigator navigator)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _prompter = prompter ?? throw new ArgumentNullException(nameof(prompter));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
    }

    /// <summary>
    ///     Displays the launch screen and executes the selected job(s).
    /// </summary>
    public void Show()
    {
        var jobs = _jobService.GetAll().OrderBy(j => j.Id).ToList();

        _console.Clear();

        if (jobs.Count == 0)
        {
            _console.WriteLine(Resources.UserInterface.Menu_Title_StartJob);
            _console.WriteLine(string.Empty);
            _console.WriteLine(Resources.UserInterface.Jobs_None);
            _prompter.Pause(Resources.UserInterface.Common_PressAnyKey);
            return;
        }

        List<Option> options =
        [
            new(Resources.UserInterface.Jobs_Execute_All, () => RunAll(jobs))
        ];
        foreach (var _job in jobs)
            options.Add(new Option(string.Format(Resources.UserInterface.Jobs_Execute_ID, _job.Id, _job.Name),
                () => RunOne(_job)));

        options.Add(new Option(Resources.UserInterface.Return, _navigator.ShowMainMenu));

        ListWidget.ShowList(options, _console, Resources.UserInterface.Menu_Title_StartJob);
    }

    /// <summary>
    ///     Executes all valid jobs.
    /// </summary>
    /// <param name="jobs">Job list.</param>
    private void RunAll(List<BackupJob> jobs)
    {
        _console.WriteLine(Resources.UserInterface.Launch_RunningAll);

        foreach (var j in jobs.OrderBy(j => j.Id))
        {
            if (!IsJobRunnable(j, out var message))
            {
                _console.WriteLine($"[{j.Id}] {message}");
                continue;
            }

            _backupService.RunJob(j);
        }

        _console.WriteLine(Resources.UserInterface.Launch_Done);
    }

    /// <summary>
    ///     Executes a specific job.
    /// </summary>
    /// <param name="job">Job to run.</param>
    private void RunOne(BackupJob job)
    {
        _console.WriteLine(string.Format(Resources.UserInterface.Launch_RunningOne, job.Id, job.Name));

        // Validate directories before launching so the UI does not report a run for invalid paths.
        if (!IsJobRunnable(job, out var message))
        {
            _console.WriteLine(message);
            return;
        }

        _backupService.RunJob(job);
        _console.WriteLine(Resources.UserInterface.Launch_Done);
    }

    private bool IsJobRunnable(BackupJob job, out string message)
    {
        var validation = _validator.Validate(job);
        if (validation.IsValid)
        {
            message = string.Empty;
            return true;
        }

        message = validation.Error switch
        {
            JobValidationError.SourceMissing => Resources.UserInterface.Path_SourceNotFound,
            JobValidationError.TargetMissing => Resources.UserInterface.Path_TargetNotFound,
            _ => Resources.UserInterface.Path_SourceNotFound
        };

        return false;
    }
}
