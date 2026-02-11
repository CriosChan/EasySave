namespace EasySave.Application.Models;

/// <summary>
///     Runtime settings for advanced backup orchestration (v3 features).
/// </summary>
public sealed class GeneralSettings
{
    public List<string> PriorityExtensions { get; set; } = [".zip", ".rar", ".7z"];
    public List<string> CryptoExtensions { get; set; } = [];
    public long LargeFileThresholdKb { get; set; } = 2048;
    public string BusinessProcessName { get; set; } = "CalculatorApp";
    public bool EnableBusinessProcessMonitor { get; set; } = true;
    public int BusinessProcessCheckIntervalMs { get; set; } = 500;
    public string CryptoSoftPath { get; set; } = "";
    public string CryptoSoftArguments { get; set; } = "\"{0}\"";

    // local | centralized | both
    public string LogMode { get; set; } = "local";
    public string CentralLogEndpoint { get; set; } = "";
}
