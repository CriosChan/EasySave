using System.Text.Json;
using EasySave.Models.State;
using EasySave.Models.Utils;

namespace EasySaveTest;

/// <summary>
///     Verifies state store thread-safety and non-destructive re-initialization behavior.
/// </summary>
[NonParallelizable]
public class StateFileSingletonThreadSafetyTests
{
    [Test]
    public void Initialize_CalledTwiceOnSamePath_DoesNotResetExistingRuntimeState()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasySaveState_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            StateFileSingleton.Instance.Initialize(root);
            var state = StateFileSingleton.Instance.GetOrCreate(50001, "Feature02.Job");

            StateFileSingleton.Instance.UpdateState(state, s =>
            {
                s.State = JobRunState.Active;
                s.ProgressPercent = 42;
                s.CurrentAction = "initial_run";
            });

            StateFileSingleton.Instance.Initialize(root);
            var reloaded = StateFileSingleton.Instance.GetOrCreate(50001, "Feature02.Job");

            Assert.Multiple(() =>
            {
                Assert.That(reloaded.State, Is.EqualTo(JobRunState.Active));
                Assert.That(reloaded.ProgressPercent, Is.EqualTo(42));
                Assert.That(reloaded.CurrentAction, Is.EqualTo("initial_run"));
            });
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Test]
    public void UpdateState_InParallel_KeepsStateFileValidAndConsistent()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasySaveState_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            StateFileSingleton.Instance.Initialize(root);
            var state = StateFileSingleton.Instance.GetOrCreate(50002, "Feature02.ConcurrentJob");

            Parallel.For(0, 250, i =>
            {
                StateFileSingleton.Instance.UpdateState(state, s =>
                {
                    s.State = JobRunState.Active;
                    s.ProgressPercent = i % 100;
                    s.RemainingFiles = 250 - i;
                    s.CurrentAction = "parallel_update";
                });
            });

            var statePath = Path.Combine(root, "state.json");
            var json = File.ReadAllText(statePath);
            var parsed = JsonSerializer.Deserialize<List<BackupJobState>>(json, JsonFile.Options);
            var target = parsed?.FirstOrDefault(x => x.JobId == 50002);

            Assert.Multiple(() =>
            {
                Assert.That(parsed, Is.Not.Null);
                Assert.That(target, Is.Not.Null);
                Assert.That(target!.CurrentAction, Is.EqualTo("parallel_update"));
                Assert.That(target.ProgressPercent, Is.GreaterThanOrEqualTo(0));
                Assert.That(target.ProgressPercent, Is.LessThan(100));
            });
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
