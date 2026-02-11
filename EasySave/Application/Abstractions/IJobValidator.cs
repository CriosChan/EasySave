using EasySave.Application.Models;
using EasySave.Domain.Models;

namespace EasySave.Application.Abstractions;

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
