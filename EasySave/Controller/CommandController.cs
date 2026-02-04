using EasySave.Models;
using EasySave.Services;
using EasySave.Utils;

namespace EasySave.Controller;

/// <summary>
/// Handles execution of backup jobs via command line arguments.
/// </summary>
public static class CommandController
{
    /// <summary>
    /// Entry point for command-line execution.
    ///
    /// Supported formats:
    /// - "1-3"  → backups 1, 2, 3
    /// - "1;3"  → backups 1 and 3
    /// - "2"    → backup 2 only
    /// </summary>
    /// <returns>Process exit code.</returns>
    public static int Run(string[] args, JobRepository repository, BackupService backupService, StateFileService stateService)
    {
        string raw = string.Join(string.Empty, args).Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            PrintUsage();
            return 1;
        }

        List<int> ids;
        try
        {
            ids = ParseArguments(raw);
        }
        catch
        {
            PrintUsage();
            return 1;
        }

        List<BackupJob> jobs = repository.Load().OrderBy(j => j.Id).ToList();
        if (jobs.Count == 0)
        {
            Console.WriteLine("No backup job configured.");
            return 1;
        }

        // Ensure the state file contains all configured jobs before running.
        stateService.Initialize(jobs);

        foreach (int id in ids)
        {
            BackupJob? job = jobs.FirstOrDefault(j => j.Id == id);
            if (job == null)
            {
                Console.WriteLine($"Job {id} not found.");
                continue;
            }

            // Validate directories before reporting a run.
            string src = PathTools.NormalizeUserPath(job.SourceDirectory);
            string dst = PathTools.NormalizeUserPath(job.TargetDirectory);
            if (string.IsNullOrWhiteSpace(src) || !Directory.Exists(src))
            {
                Console.WriteLine($"Job {job.Id} skipped: source directory not found.");
                continue;
            }
            if (string.IsNullOrWhiteSpace(dst) || !Directory.Exists(dst))
            {
                Console.WriteLine($"Job {job.Id} skipped: target directory not found.");
                continue;
            }

            Console.WriteLine($"Running job {job.Id} - {job.Name}...");
            backupService.RunJob(job);
        }

        return 0;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  EasySave.exe 1-3");
        Console.WriteLine("  EasySave.exe 1;3");
        Console.WriteLine("  EasySave.exe 2");
    }

    private static List<int> ParseArguments(string arg)
    {
        var result = new List<int>();
        string normalized = arg.Replace(" ", string.Empty);

        if (normalized.Contains('-'))
        {
            var parts = normalized.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new FormatException();

            int start = int.Parse(parts[0]);
            int end = int.Parse(parts[1]);
            if (start <= 0 || end <= 0)
                throw new FormatException();

            if (end < start)
            {
                (start, end) = (end, start);
            }

            for (int i = start; i <= end; i++)
                result.Add(i);
        }
        else if (normalized.Contains(';'))
        {
            result = normalized
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();
        }
        else
        {
            result.Add(int.Parse(normalized));
        }

        // Avoid duplicates while keeping the initial order.
        return result.Distinct().ToList();
    }
}
