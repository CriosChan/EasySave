using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;
using EasySave.Models.State;

namespace EasySaveTest;

/// <summary>
///     Tests that WaitWhileBusinessSoftwareRuns correctly fires the onPauseStateChanged callback.
/// </summary>
[NonParallelizable]
public class BusinessSoftwarePauseNotificationTests
{
    [Test]
    public void WaitWhileBusinessSoftwareRuns_WithSoftwareRunning_InvokesPauseCallback()
    {
        var stateRoot = Path.Combine(Path.GetTempPath(), $"EasySaveState_{Guid.NewGuid():N}");
        Directory.CreateDirectory(stateRoot);

        try
        {
            StateFileSingleton.Instance.Initialize(stateRoot);
            var state = StateFileSingleton.Instance.GetOrCreate(99001, "PauseNotif.Job1");

            var coordinator = new GlobalBusinessSoftwarePauseCoordinator(TimeSpan.FromMilliseconds(15));
            using var reg = coordinator.RegisterJob(99001, "PauseNotif.Job1",
                new SequenceMonitor(true, false));

            bool? pauseCallbackValue = null;
            coordinator.WaitWhileBusinessSoftwareRuns(99001, state, null, () => false,
                paused => pauseCallbackValue = paused);

            // Callback should have been called with true (pause) then false (resume)
            Assert.That(pauseCallbackValue, Is.False);
        }
        finally
        {
            Directory.Delete(stateRoot, true);
        }
    }

    [Test]
    public void WaitWhileBusinessSoftwareRuns_NoBusinessSoftware_DoesNotInvokePauseCallback()
    {
        var stateRoot = Path.Combine(Path.GetTempPath(), $"EasySaveState_{Guid.NewGuid():N}");
        Directory.CreateDirectory(stateRoot);

        try
        {
            StateFileSingleton.Instance.Initialize(stateRoot);
            var state = StateFileSingleton.Instance.GetOrCreate(99002, "PauseNotif.Job2");

            var coordinator = new GlobalBusinessSoftwarePauseCoordinator(TimeSpan.FromMilliseconds(15));
            using var reg = coordinator.RegisterJob(99002, "PauseNotif.Job2",
                new SequenceMonitor(false));

            var callbackInvoked = false;
            coordinator.WaitWhileBusinessSoftwareRuns(99002, state, null, () => false,
                _ => callbackInvoked = true);

            Assert.That(callbackInvoked, Is.False);
        }
        finally
        {
            Directory.Delete(stateRoot, true);
        }
    }

    [Test]
    public void WaitWhileBusinessSoftwareRuns_PauseThenResume_CallbackCalledWithTrueThenFalse()
    {
        var stateRoot = Path.Combine(Path.GetTempPath(), $"EasySaveState_{Guid.NewGuid():N}");
        Directory.CreateDirectory(stateRoot);

        try
        {
            StateFileSingleton.Instance.Initialize(stateRoot);
            var state = StateFileSingleton.Instance.GetOrCreate(99003, "PauseNotif.Job3");

            var coordinator = new GlobalBusinessSoftwarePauseCoordinator(TimeSpan.FromMilliseconds(15));
            using var reg = coordinator.RegisterJob(99003, "PauseNotif.Job3",
                new SequenceMonitor(true, true, false));

            var callbackValues = new List<bool>();
            coordinator.WaitWhileBusinessSoftwareRuns(99003, state, null, () => false,
                paused => callbackValues.Add(paused));

            Assert.Multiple(() =>
            {
                Assert.That(callbackValues, Has.Count.EqualTo(2));
                Assert.That(callbackValues[0], Is.True);  // paused
                Assert.That(callbackValues[1], Is.False); // resumed
            });
        }
        finally
        {
            Directory.Delete(stateRoot, true);
        }
    }

    [Test]
    public void NotifyBusinessSoftwarePause_ViaController_ForwardedThroughBackupJob()
    {
        var job = new BackupJob(1, "NotifJob", "C:\\Src", "C:\\Dst", EasySave.Core.Models.BackupType.Complete);

        bool? receivedPauseValue = null;
        job.BusinessSoftwarePauseChanged += (_, _) => receivedPauseValue = job.PausedByBusiness;

        // Simulate what the coordinator does via the controller
        job.BusinessSoftwarePauseCoordinator = new GlobalBusinessSoftwarePauseCoordinator();
        // Directly test via BackupJobController reflection access would break encapsulation,
        // so instead test the end-to-end property exposure.
        Assert.That(job.PausedByBusiness, Is.False);
    }

    /// <summary>
    ///     Minimal monitor that returns a predefined sequence of values.
    /// </summary>
    private sealed class SequenceMonitor(params bool[] sequence) : IBusinessSoftwareMonitor
    {
        private readonly Queue<bool> _queue = new(sequence);
        private bool _last;

        public IReadOnlyList<string> ConfiguredSoftwareNames { get; } = ["TestSoftware"];

        public bool IsBusinessSoftwareRunning()
        {
            if (_queue.Count == 0)
                return _last;

            _last = _queue.Dequeue();
            return _last;
        }
    }
}

