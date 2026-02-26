using EasySave.Models.Backup.Abstractions;
using EasySave.Models.State;
using EasySave.Models.Utils;

namespace EasySaveTest;

[NonParallelizable]
public class StateLoggerBusinessSoftwareTests
{
    [Test]
    public void SetStatePausedBusinessSoftware_UpdatesStateWithExpectedReasonAndPaths()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasySaveState_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            StateFileSingleton.Instance.Initialize(root);
            var state = StateFileSingleton.Instance.GetOrCreate(50009, "Feature09.Job");
            var file = new FakeFile(@"C:\source\locked.bin", @"C:\target\locked.bin");

            StateLogger.SetStatePausedBusinessSoftware(state, file);

            var reloaded = StateFileSingleton.Instance.GetOrCreate(50009, "Feature09.Job");
            Assert.Multiple(() =>
            {
                Assert.That(reloaded.State, Is.EqualTo(JobRunState.PausedBusinessSoftware));
                Assert.That(reloaded.CurrentAction, Is.EqualTo("Paused: business software running"));
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
        public FakeFile(string sourceFile, string targetFile)
        {
            SourceFile = sourceFile;
            TargetFile = targetFile;
        }

        public string SourceFile { get; }
        public string TargetFile { get; }
        public void Copy() { }
        public long GetSize() => 0;
    }
}
