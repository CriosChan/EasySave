using EasySave.Models;
using EasySave.Services;
using EasySave.Utils;

namespace EasySave.View;

public static class UserInterface
{
    private static JobRepository? _repository;
    private static BackupService? _backupService;
    private static StateFileService? _stateService;

    public static void Initialize(JobRepository repository, BackupService backupService, StateFileService stateService)
    {
        _repository = repository;
        _backupService = backupService;
        _stateService = stateService;
    }

    /// <summary>
    /// Show the main menu to the user.
    /// </summary>
    public static void ShowMenu()
    {
        EnsureInitialized();

        ListWidget.ShowList(
        [
            new Option(Ressources.UserInterface.Menu_ListJobs, ListJobs),
            new Option(Ressources.UserInterface.Menu_AddBackup, AddJob),
            new Option(Ressources.UserInterface.Menu_RemoveBackup, RemoveJob),
            new Option(Ressources.UserInterface.Menu_LaunchBackupJob, LaunchJob)
        ]);
    }

    private static void ListJobs()
    {
        EnsureInitialized();
        List<BackupJob> jobs = _repository!.Load().OrderBy(j => j.Id).ToList();

        Console.Clear();
        Console.WriteLine(Text.Get("Jobs.Header"));
        Console.WriteLine();

        if (jobs.Count == 0)
        {
            Console.WriteLine(Text.Get("Jobs.None"));
            Pause();
            return;
        }

        Console.WriteLine(Text.Get("Jobs.Columns"));
        Console.WriteLine(new string('-', 80));

        foreach (BackupJob job in jobs)
        {
            Console.WriteLine($"{job.Id,2} | {job.Name,-20} | {job.Type,-12} | {job.SourceDirectory} -> {job.TargetDirectory}");
        }

        Pause();
    }

    private static void AddJob()
    {
        EnsureInitialized();
        List<BackupJob> jobs = _repository!.Load();

        Console.Clear();
        Console.WriteLine(Text.Get("Add.Header"));
        Console.WriteLine();

        string name = ReadNonEmpty(Text.Get("Add.PromptName"));
        string source = ReadExistingDirectory(Text.Get("Add.PromptSource"), "Path.SourceNotFound");
        string target = ReadExistingDirectory(Text.Get("Add.PromptTarget"), "Path.TargetNotFound");

        BackupType type = ReadBackupType();

        BackupJob newJob = new BackupJob
        {
            Name = name,
            SourceDirectory = source,
            TargetDirectory = target,
            Type = type
        };

        (bool ok, string error) = _repository.AddJob(jobs, newJob);

        if (!ok)
        {
            Console.WriteLine();
            Console.WriteLine(Text.Get("Add.Failed"));
            Console.WriteLine(TranslateRepositoryError(error));
            Pause();
            return;
        }

        RefreshStateFile();

        Console.WriteLine();
        Console.WriteLine(Text.Get("Add.Success"));
        Pause();
    }

    private static void RemoveJob()
    {
        EnsureInitialized();
        List<BackupJob> jobs = _repository!.Load();

        Console.Clear();
        Console.WriteLine(Text.Get("Remove.Header"));
        Console.WriteLine();

        if (jobs.Count == 0)
        {
            Console.WriteLine(Text.Get("Jobs.None"));
            Pause();
            return;
        }

        Console.WriteLine(Text.Get("Remove.Prompt"));
        string? input = Console.ReadLine();
        input = (input ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine(Text.Get("Common.Cancelled"));
            Pause();
            return;
        }

        bool removed = _repository.RemoveJob(jobs, input);
        if (!removed)
        {
            Console.WriteLine(Text.Get("Remove.NotFound"));
            Pause();
            return;
        }

        RefreshStateFile();
        Console.WriteLine(Text.Get("Remove.Success"));
        Pause();
    }

    private static void LaunchJob()
    {
        EnsureInitialized();
        List<BackupJob> jobs = _repository!.Load().OrderBy(j => j.Id).ToList();

        Console.Clear();
        Console.WriteLine(Text.Get("Launch.Header"));
        Console.WriteLine();

        if (jobs.Count == 0)
        {
            Console.WriteLine(Text.Get("Jobs.None"));
            Pause();
            return;
        }

        Console.WriteLine(Text.Get("Launch.Prompt"));
        string? input = Console.ReadLine();
        input = (input ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine(Text.Get("Common.Cancelled"));
            Pause();
            return;
        }

        // Always re-initialize the state file so it contains all jobs before execution.
        _stateService!.Initialize(jobs);

        if (input == "0")
        {
            Console.WriteLine(Text.Get("Launch.RunningAll"));
            // Skip invalid jobs so we don't report a run when paths are incorrect.
            foreach (BackupJob j in jobs.OrderBy(j => j.Id))
            {
                if (!PathTools.TryNormalizeExistingDirectory(j.SourceDirectory, out _))
                {
                    Console.WriteLine($"[{j.Id}] {Text.Get("Path.SourceNotFound")}");
                    continue;
                }
                if (!PathTools.TryNormalizeExistingDirectory(j.TargetDirectory, out _))
                {
                    Console.WriteLine($"[{j.Id}] {Text.Get("Path.TargetNotFound")}");
                    continue;
                }

                _backupService!.RunJob(j);
            }
            Console.WriteLine(Text.Get("Launch.Done"));
            Pause();
            return;
        }

        if (!int.TryParse(input, out int id))
        {
            Console.WriteLine(Text.Get("Launch.Invalid"));
            Pause();
            return;
        }

        BackupJob? job = jobs.FirstOrDefault(j => j.Id == id);
        if (job == null)
        {
            Console.WriteLine(Text.Get("Launch.NotFound"));
            Pause();
            return;
        }

        Console.WriteLine(string.Format(Text.Get("Launch.RunningOne"), job.Id, job.Name));

        // Validate directories before launching so the UI does not report a run for invalid paths.
        if (!PathTools.TryNormalizeExistingDirectory(job.SourceDirectory, out _))
        {
            Console.WriteLine(Text.Get("Path.SourceNotFound"));
            Pause();
            return;
        }
        if (!PathTools.TryNormalizeExistingDirectory(job.TargetDirectory, out _))
        {
            Console.WriteLine(Text.Get("Path.TargetNotFound"));
            Pause();
            return;
        }

        _backupService!.RunJob(job);
        Console.WriteLine(Text.Get("Launch.Done"));
        Pause();
    }

    private static string ReadExistingDirectory(string prompt, string notFoundKey)
    {
        while (true)
        {
            string raw = ReadNonEmpty(prompt);
            if (PathTools.TryNormalizeExistingDirectory(raw, out _))
                return raw;

            Console.WriteLine(Text.Get(notFoundKey));
        }
    }

    private static void RefreshStateFile()
    {
        // Update state.json so it always reflects the current job list.
        List<BackupJob> jobs = _repository!.Load().OrderBy(j => j.Id).ToList();
        _stateService!.Initialize(jobs);
    }

    private static string TranslateRepositoryError(string errorCode)
    {
        return errorCode switch
        {
            "Error.MaxJobs" => Text.Get("Add.Error.MaxJobs"),
            "Error.NoFreeSlot" => Text.Get("Add.Error.NoFreeSlot"),
            _ => errorCode
        };
    }

    private static BackupType ReadBackupType()
    {
        while (true)
        {
            Console.WriteLine(Text.Get("Add.PromptType"));
            Console.WriteLine(Text.Get("Add.TypeOptions"));
            string? raw = Console.ReadLine();
            raw = (raw ?? string.Empty).Trim();

            if (raw == "1")
                return BackupType.Complete;
            if (raw == "2")
                return BackupType.Differential;

            Console.WriteLine(Text.Get("Common.InvalidInput"));
        }
    }

    private static string ReadNonEmpty(string prompt)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string? raw = Console.ReadLine();
            raw = (raw ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(raw))
                return raw;

            Console.WriteLine(Text.Get("Common.InvalidInput"));
        }
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine(Text.Get("Common.PressAnyKey"));
        Console.ReadKey(true);
    }

    private static void EnsureInitialized()
    {
        if (_repository == null || _backupService == null || _stateService == null)
            throw new InvalidOperationException("UserInterface.Initialize must be called before ShowMenu.");
    }
}