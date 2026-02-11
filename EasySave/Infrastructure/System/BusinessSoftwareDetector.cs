using System.Diagnostics;
using EasySave.Application.Abstractions;

namespace EasySave.Infrastructure.System;

/// <summary>
///     Process-based implementation for business software detection.
/// </summary>
public sealed class BusinessSoftwareDetector : IBusinessSoftwareDetector
{
    public bool IsRunning(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
            return false;

        var normalized = Path.GetFileNameWithoutExtension(processName.Trim());
        if (string.IsNullOrWhiteSpace(normalized))
            return false;

        try
        {
            return Process.GetProcessesByName(normalized).Length > 0;
        }
        catch
        {
            return false;
        }
    }
}
