using EasySave.Core.Models;
using EasySave.Models.Backup;

namespace EasySaveTest;

/// <summary>
///     Tests for the shared backup execution engine used by GUI and CLI entry points.
/// </summary>
public class BackupExecutionEngineTests
{
    [Test]
    public async Task ExecuteJobAsync_WithValidJob_CopiesFilesAndReturnsResult()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasySaveTest_{Guid.NewGuid():N}");
        var source = Path.Combine(root, "source");
        var target = Path.Combine(root, "target");
        var fileName = "f01-engine.__f01test";
        var sourceFile = Path.Combine(source, fileName);
        var targetFile = Path.Combine(target, fileName);

        Directory.CreateDirectory(source);
        Directory.CreateDirectory(target);
        File.WriteAllText(sourceFile, "feature-01");

        try
        {
            var job = new BackupJob("feature-01-job", source, target, BackupType.Complete);
            var engine = new BackupExecutionEngine();

            var result = await engine.ExecuteJobAsync(job);

            Assert.Multiple(() =>
            {
                Assert.That(result.JobName, Is.EqualTo(job.Name));
                Assert.That(result.JobId, Is.EqualTo(job.Id));
                Assert.That(result.WasStoppedByBusinessSoftware, Is.False);
                Assert.That(File.Exists(targetFile), Is.True);
            });
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Test]
    public async Task ExecuteJobAsync_WithProgressSink_EmitsProgressSnapshots()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasySaveTest_{Guid.NewGuid():N}");
        var source = Path.Combine(root, "source");
        var target = Path.Combine(root, "target");

        Directory.CreateDirectory(source);
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(source, "a.__f01test"), "a");
        File.WriteAllText(Path.Combine(source, "b.__f01test"), "bb");

        try
        {
            var job = new BackupJob("feature-01-progress", source, target, BackupType.Complete);
            var engine = new BackupExecutionEngine();
            var collector = new SnapshotCollector();

            await engine.ExecuteJobAsync(job, collector);

            Assert.Multiple(() =>
            {
                Assert.That(collector.Count, Is.GreaterThan(0));
                Assert.That(collector.LastSnapshot, Is.Not.Null);
                Assert.That(collector.LastSnapshot!.JobName, Is.EqualTo(job.Name));
            });
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Test]
    public void ExecuteJobAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasySaveTest_{Guid.NewGuid():N}");
        var source = Path.Combine(root, "source");
        var target = Path.Combine(root, "target");

        Directory.CreateDirectory(source);
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(source, "cancel.__f01test"), "cancel");

        try
        {
            var job = new BackupJob("feature-01-cancel", source, target, BackupType.Complete);
            var engine = new BackupExecutionEngine();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await engine.ExecuteJobAsync(job, cancellationToken: cts.Token));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private sealed class SnapshotCollector : IProgress<BackupExecutionProgressSnapshot>
    {
        public int Count { get; private set; }
        public BackupExecutionProgressSnapshot? LastSnapshot { get; private set; }

        public void Report(BackupExecutionProgressSnapshot value)
        {
            Count++;
            LastSnapshot = value;
        }
    }
}
