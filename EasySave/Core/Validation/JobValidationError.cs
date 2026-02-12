namespace EasySave.Core.Validation;

/// <summary>
///     Represents validation errors for a backup job.
/// </summary>
public enum JobValidationError
{
    None = 0,
    SourceMissing = 1,
    TargetMissing = 2
}
