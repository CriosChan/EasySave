namespace EasySave.Application.Abstractions;

/// <summary>
///     Detects whether the configured business process is running.
/// </summary>
public interface IBusinessSoftwareDetector
{
    bool IsRunning(string processName);
}
