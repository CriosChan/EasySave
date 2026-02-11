namespace EasySave.Application.Models;

/// <summary>
///     Result of validating a backup job.
/// </summary>
public readonly record struct JobValidationResult(
    bool IsValid,
    string SourceDirectory,
    string TargetDirectory,
    JobValidationError Error)
{
    public static JobValidationResult Valid(string sourceDirectory, string targetDirectory)
        => new(true, sourceDirectory, targetDirectory, JobValidationError.None);

    public static JobValidationResult Invalid(
        JobValidationError error,
        string sourceDirectory,
        string targetDirectory)
        => new(false, sourceDirectory, targetDirectory, error);
}
