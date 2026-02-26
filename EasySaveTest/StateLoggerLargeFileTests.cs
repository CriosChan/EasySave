using EasySave.Models.Backup.Abstractions;
using EasySave.Models.State;
using EasySave.Models.Utils;

namespace EasySaveTest;

[NonParallelizable]
public class StateLoggerLargeFileTests
{
    [Test]
    public void SetStateWaitingLargeFile_UpdatesStateWithExpectedReasonAndPaths()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasySaveState_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            StateFileSingleton.Instance.Initialize(root);
            var state = StateFileSingleton.Instance.GetOrCreate(50008, "Feature08.Job");
            var file = new FakeFile(@"C:\source\large.bin", @"C:\target\large.bin", 42_000);

            StateLogger.SetStateWaitingLargeFile(state, file);

            var reloaded = StateFileSingleton.Instance.GetOrCreate(50008, "Feature08.Job");
            Assert.Multiple(() =>
            {
                Assert.That(reloaded.State, Is.EqualTo(JobRunState.WaitingLargeFile));
                Assert.That(reloaded.CurrentAction, Is.EqualTo("Waiting for large-file transfer slot"));
                Assert.That(reloaded.CurrentSourcePath, Is.EqualTo(PathService.ToFullUncLikePath(file.SourceFile)));
                Assert.That(reloaded.CurrentTargetPath, Is.EqualTo(PathService.ToFullUncLikePath(file.TargetFile)));
            });
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private sealed class FakeFile : IFile
    {
        private readonly long _sizeBytes;

        public FakeFile(string sourceFile, string targetFile, long sizeBytes)
        {
            SourceFile = sourceFile;
            TargetFile = targetFile;
            _sizeBytes = sizeBytes;
        }

        public string SourceFile { get; }
        public string TargetFile { get; }
        public void Copy() { }
        public long GetSize() => _sizeBytes;
    }
}
