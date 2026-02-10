using EasySave.Application.Abstractions;
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
    private readonly IPathService _paths;
    private readonly ConsolePrompter _prompter;
    private readonly IJobRepository _repository;
    private readonly IStateService _stateService;

    /// <summary>
    ///     Builds the launch view.
    /// </summary>
    /// <param name="console">Target console.</param>
    /// <param name="repository">Job repository.</param>
    /// <param name="backupService">Backup service.</param>
    /// <param name="stateService">State service.</param>
    /// <param name="prompter">Input prompter.</param>
    /// <param name="paths">Path service.</param>
    public JobLaunchView(
        IConsole console,
        IJobRepository repository,
        IBackupService backupService,
        IStateService stateService,
        ConsolePrompter prompter,
        IPathService paths)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
        _prompter = prompter ?? throw new ArgumentNullException(nameof(prompter));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    /// <summary>
    ///     Displays the launch screen and executes the selected job(s).
    /// </summary>
    public void Show()
    {
        var jobs = _repository.Load().OrderBy(j => j.Id).ToList();

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

        options.Add(new Option(Resources.UserInterface.Return, UserInterface.ShowMenu));

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
            if (!_paths.TryNormalizeExistingDirectory(j.SourceDirectory, out _))
            {
                _console.WriteLine($"[{j.Id}] {Resources.UserInterface.Path_SourceNotFound}");
                continue;
            }

            if (!_paths.TryNormalizeExistingDirectory(j.TargetDirectory, out _))
            {
                _console.WriteLine($"[{j.Id}] {Resources.UserInterface.Path_TargetNotFound}");
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
        if (!_paths.TryNormalizeExistingDirectory(job.SourceDirectory, out _))
        {
            _console.WriteLine(Resources.UserInterface.Path_SourceNotFound);
            return;
        }

        if (!_paths.TryNormalizeExistingDirectory(job.TargetDirectory, out _))
        {
            _console.WriteLine(Resources.UserInterface.Path_TargetNotFound);
            return;
        }

        _backupService.RunJob(job);
        _console.WriteLine(Resources.UserInterface.Launch_Done);
    }
}