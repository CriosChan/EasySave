using System.Diagnostics;
using EasySave.Core.Models;
using EasySave.Models.Backup.Abstractions;
using EasySave.Models.State;

namespace EasySaveTest;

/// <summary>
///     Tests business software blocking behavior during backup execution.
/// </summary>
[NonParallelizable]
public class BusinessSoftwareBlockingTest
{
    [Test]
    public void Coordinator_WaitsWhenAnotherRegisteredJobDetectsBusinessSoftware()
    {
        var stateRoot = Path.Combine(Path.GetTempPath(), $"EasySaveState_{Guid.NewGuid():N}");
        Directory.CreateDirectory(stateRoot);

        try
        {
            StateFileSingleton.Instance.Initialize(stateRoot);
            var state = StateFileSingleton.Instance.GetOrCreate(50109, "Feature09.Job2");

            var coordinator = new GlobalBusinessSoftwarePauseCoordinator(TimeSpan.FromMilliseconds(15));
            using var registration1 = coordinator.RegisterJob(
                50108,
                "Feature09.Job1",
                new SequenceBusinessSoftwareMonitor(true, true, false));
            using var registration2 = coordinator.RegisterJob(
                50109,
                "Feature09.Job2",
                new SequenceBusinessSoftwareMonitor(false, false, false));

            var startedAt = Stopwatch.StartNew();
            coordinator.WaitWhileBusinessSoftwareRuns(50109, state, null, () => false);
            startedAt.Stop();

            Assert.Multiple(() =>
            {
                Assert.That(startedAt.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(20));
                Assert.That(state.State, Is.EqualTo(JobRunState.PausedBusinessSoftware));
            });
        }
        finally
        {
            Directory.Delete(stateRoot, true);
        }
    }

    [Test]
    public async Task ExecuteJobAsync_AutoPausesThenResumes_InsteadOfStopping()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasySaveTest_{Guid.NewGuid():N}");
        var source = Path.Combine(root, "source");
        var target = Path.Combine(root, "target");
        var sourceFile = Path.Combine(source, "feature09-blocked.dat");
        var targetFile = Path.Combine(target, "feature09-blocked.dat");

        Directory.CreateDirectory(source);
        Directory.CreateDirectory(target);
        File.WriteAllText(sourceFile, "feature-09");

        try
        {
            var job = new BackupJob("feature-09-job", source, target, BackupType.Complete)
            {
                BusinessSoftwareMonitor = new SequenceBusinessSoftwareMonitor(true, true, false),
                BusinessSoftwarePauseCoordinator =
                    new GlobalBusinessSoftwarePauseCoordinator(TimeSpan.FromMilliseconds(15))
            };

            var engine = new BackupExecutionEngine();
            var startedAt = Stopwatch.StartNew();
            var result = await engine.ExecuteJobAsync(job);
            startedAt.Stop();

            Assert.Multiple(() =>
            {
                Assert.That(startedAt.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(20));
                Assert.That(result.WasStoppedByBusinessSoftware, Is.False);
                Assert.That(File.Exists(targetFile), Is.True);
            });
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
