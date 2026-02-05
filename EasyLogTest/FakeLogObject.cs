namespace EasyLogTest;

/// <summary>
/// Objet de log factice utilise pour les tests.
/// </summary>
public struct FakeLogObject(
    String name,
    String fileSource,
    String fileTarget,
    int fileSize,
    int fileTransferTime,
    DateTime time)
{
    public String Name { get; set; } = name;
    public String FileSource { get; set; } = fileSource;
    public String FileTarget { get; set; } = fileTarget;
    public int FileSize { get; set; } = fileSize;
    public int FileTransferTime { get; set; } = fileTransferTime;
    public DateTime Time { get; set; } = time;
}
