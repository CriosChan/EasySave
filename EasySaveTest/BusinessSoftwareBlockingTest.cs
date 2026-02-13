using EasySave.Core.Models;
using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;

namespace EasySaveTest;

public class BusinessSoftwareBlockingTest
{
    [Test]
    public void StartBackup_WhenBusinessSoftwareAlreadyRunning_DoesNotCopyFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasySaveTest_{Guid.NewGuid():N}");
        var source = Path.Combine(root, "source");
        var target = Path.Combine(root, "target");

        Directory.CreateDirectory(source);
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(source, "a.txt"), "content-a");

        try
        {
            var job = new BackupJob("blocked-before-start", source, target, BackupType.Complete)
            {
                BusinessSoftwareMonitor = new SequenceBusinessSoftwareMonitor(true)
            };

            job.StartBackup();

            Assert.That(job.WasStoppedByBusinessSoftware, Is.True);
            Assert.That(Directory.GetFiles(target, "*", SearchOption.AllDirectories).Length, Is.EqualTo(0));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Test]
    public void StartBackup_WhenBusinessSoftwareAppearsDuringSequentialRun_StopsAfterCurrentFile()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasySaveTest_{Guid.NewGuid():N}");
        var source = Path.Combine(root, "source");
        var target = Path.Combine(root, "target");

        Directory.CreateDirectory(source);
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(source, "a.txt"), "content-a");
        File.WriteAllText(Path.Combine(source, "b.txt"), "content-b");

        try
        {
            var job = new BackupJob("blocked-during-sequence", source, target, BackupType.Complete)
            {
                BusinessSoftwareMonitor = new SequenceBusinessSoftwareMonitor(false, true)
            };

            job.StartBackup();

            Assert.That(job.WasStoppedByBusinessSoftware, Is.True);
            Assert.That(Directory.GetFiles(target, "*", SearchOption.AllDirectories).Length, Is.EqualTo(1));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private sealed class SequenceBusinessSoftwareMonitor(params bool[] sequence) : IBusinessSoftwareMonitor
    {
        private readonly Queue<bool> _sequence = new(sequence);
        private bool _last;

        public string ConfiguredSoftwareName => "CalculatorApp";

        public bool IsBusinessSoftwareRunning()
        {
            if (_sequence.Count == 0)
                return _last;

            _last = _sequence.Dequeue();
            return _last;
        }
    }
}
