using EasySave.Core.Contracts;
using EasySave.Core.Validation;
using EasySave.Core.Models;

namespace EasySave.Services;

/// <summary>
///     Validates job paths using the path service.
/// </summary>
public sealed class JobValidator : IJobValidator
{
    private readonly IPathService _paths;

    public JobValidator(IPathService paths)
    {
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    public JobValidationResult Validate(BackupJob job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        var sourceOk = _paths.TryNormalizeExistingDirectory(job.SourceDirectory, out var sourceDir);
        var targetOk = _paths.TryNormalizeExistingDirectory(job.TargetDirectory, out var targetDir);

        if (!sourceOk)
            return JobValidationResult.Invalid(JobValidationError.SourceMissing, sourceDir, targetDir);

        if (!targetOk)
            return JobValidationResult.Invalid(JobValidationError.TargetMissing, sourceDir, targetDir);

        return JobValidationResult.Valid(sourceDir, targetDir);
    }
}
