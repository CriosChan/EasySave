namespace EasyLogTest;

/// <summary>
///     Fake log object used for tests.
/// </summary>
public struct FakeLogObject(
    string name,
    string fileSource,
    string fileTarget,
    int fileSize,
    int fileTransferTime,
    DateTime time)
{
    public string Name { get; set; } = name;
    public string FileSource { get; set; } = fileSource;
    public string FileTarget { get; set; } = fileTarget;
    public int FileSize { get; set; } = fileSize;
    public int FileTransferTime { get; set; } = fileTransferTime;
    public DateTime Time { get; set; } = time;
}