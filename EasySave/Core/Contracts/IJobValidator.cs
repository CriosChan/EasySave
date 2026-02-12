using EasySave.Core.Validation;
using EasySave.Core.Models;

namespace EasySave.Core.Contracts;

/// <summary>
///     Contract for validating job inputs (paths, etc.).
/// </summary>
public interface IJobValidator
{
    /// <summary>
    ///     Validates a job and returns normalized paths when possible.
    /// </summary>
    /// <param name="job">Job to validate.</param>
    /// <returns>Validation result.</returns>
    JobValidationResult Validate(BackupJob job);
}
