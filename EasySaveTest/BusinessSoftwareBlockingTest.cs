using EasySave.Core.Models;
using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;

namespace EasySaveTest;

/// <summary>
///     Tests business software blocking behavior during backup execution.
/// </summary>
public class BusinessSoftwareBlockingTest
{
    /// <summary>
    ///     Verifies that no file is copied when business software is already running before job start.
    /// </summary>
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

    /// <summary>
    ///     Verifies that sequential processing stops before the next file when business software appears.
    /// </summary>
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

    /// <summary>
    ///     Test monitor that returns a predefined running/not-running sequence.
    /// </summary>
    /// <param name="sequence">Sequence of values returned by process checks.</param>
    private sealed class SequenceBusinessSoftwareMonitor(params bool[] sequence) : IBusinessSoftwareMonitor
    {
        private readonly Queue<bool> _sequence = new(sequence);
        private bool _last;

        /// <summary>
        ///     Gets the configured software names used for assertions and logs.
        /// </summary>
        public IReadOnlyList<string> ConfiguredSoftwareNames { get; } = ["CalculatorApp"];

        /// <summary>
        ///     Returns the next configured value, then repeats the last known value.
        /// </summary>
        /// <returns>True when business software should be considered running.</returns>
        public bool IsBusinessSoftwareRunning()
        {
            if (_sequence.Count == 0)
                return _last;

            _last = _sequence.Dequeue();
            return _last;
        }
    }
}
