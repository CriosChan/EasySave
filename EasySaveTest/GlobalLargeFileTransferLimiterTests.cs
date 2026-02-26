
namespace EasySaveTest;

[TestFixture]
public class GlobalLargeFileTransferLimiterTests
{
    [Test]
    public void RequiresExclusiveSlot_UsesStrictlyGreaterThanThreshold()
    {
        var limiter = new GlobalLargeFileTransferLimiter(() => 100);

        Assert.Multiple(() =>
        {
            Assert.That(limiter.RequiresExclusiveSlot((100 * 1024) - 1), Is.False);
            Assert.That(limiter.RequiresExclusiveSlot(100 * 1024), Is.False);
            Assert.That(limiter.RequiresExclusiveSlot((100 * 1024) + 1), Is.True);
        });
    }

    [Test]
    public void TryAcquireExclusiveSlot_AllowsOnlyOneHolderAtATime()
    {
        var limiter = new GlobalLargeFileTransferLimiter(() => 1);

        Assert.That(limiter.TryAcquireExclusiveSlot(TimeSpan.FromMilliseconds(50)), Is.True);

        try
        {
            var acquiredWhileLocked = limiter.TryAcquireExclusiveSlot(TimeSpan.FromMilliseconds(100));
            Assert.That(acquiredWhileLocked, Is.False);
        }
        finally
        {
            limiter.ReleaseExclusiveSlot();
        }

        Assert.That(limiter.TryAcquireExclusiveSlot(TimeSpan.FromMilliseconds(50)), Is.True);
        limiter.ReleaseExclusiveSlot();
    }

    [Test]
    public void ConcurrentLargeTransfers_AreSerialized()
    {
        var limiter = new GlobalLargeFileTransferLimiter(() => 1);
        var concurrentTransfers = 0;
        var maxConcurrentTransfers = 0;

        void RunLargeTransfer()
        {
            Assert.That(limiter.TryAcquireExclusiveSlot(TimeSpan.FromSeconds(1)), Is.True);
            try
            {
                var current = Interlocked.Increment(ref concurrentTransfers);
                UpdateMax(ref maxConcurrentTransfers, current);
                Thread.Sleep(120);
            }
            finally
            {
                Interlocked.Decrement(ref concurrentTransfers);
                limiter.ReleaseExclusiveSlot();
            }
        }

        var tasks = new[]
        {
            Task.Run(RunLargeTransfer),
            Task.Run(RunLargeTransfer),
            Task.Run(RunLargeTransfer)
        };

        Task.WaitAll(tasks);
        Assert.That(maxConcurrentTransfers, Is.EqualTo(1));
    }

    private static void UpdateMax(ref int maxValue, int candidate)
    {
        while (true)
        {
            var snapshot = maxValue;
            if (snapshot >= candidate)
                return;

            if (Interlocked.CompareExchange(ref maxValue, candidate, snapshot) == snapshot)
                return;
        }
    }
}
