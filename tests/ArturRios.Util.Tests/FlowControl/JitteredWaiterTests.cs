using System.Diagnostics;
using ArturRios.Util.FlowControl.Waiter;

namespace ArturRios.Util.Tests.FlowControl;

public class JitteredWaiterTests
{
    [Theory]
    [InlineData(0, 500, 500)]
    [InlineData(1, 1250, 1999)]
    [InlineData(2, 2250, 3999)]
    public async Task GivenAttemptIndex_WhenWaitAsync_ThenElapsedWithinExpectedRange(int attemptIndex, int expectedMinMs, int expectedMaxMs)
    {
        var waiter = new JitteredWaiter(maxRetryCount: 10);

        for (var i = 0; i < attemptIndex; i++)
        {
            await waiter.WaitAsync();
        }

        var sw = Stopwatch.StartNew();
        await waiter.WaitAsync();

        sw.Stop();

        var elapsed = sw.ElapsedMilliseconds;

        var lowerBound = System.Math.Max(0, expectedMinMs - 150);
        var upperBound = expectedMaxMs + 350;

        Assert.InRange(elapsed, lowerBound, upperBound);
    }

    [Fact]
    public async Task GivenMaxRetryCount_WhenWaitAsync_ThenUpdateCanRetryPropertyCorrectly()
    {
        var waiter = new JitteredWaiter(2);

        Assert.True(waiter.CanRetry);

        await waiter.WaitAsync();

        Assert.True(waiter.CanRetry);

        await waiter.WaitAsync();

        Assert.False(waiter.CanRetry);
    }

    [Fact]
    public async Task GivenMaxRetryCount_WhenExceedingRetries_ThenThrowException()
    {
        var waiter = new JitteredWaiter(2);

        await waiter.WaitAsync();
        await waiter.WaitAsync();

        await Assert.ThrowsAsync<MaxRetriesReachedException>(() => waiter.WaitAsync());
    }

    [Fact]
    public async Task GivenZeroMaxRetryCount_WhenWaitAsync_ThenThrowImmediately()
    {
        var waiter = new JitteredWaiter(0);

        Assert.False(waiter.CanRetry);

        await Assert.ThrowsAsync<MaxRetriesReachedException>(() => waiter.WaitAsync());
    }

    [Fact]
    public async Task GivenHighRetryCount_WhenAttemptIndexWouldOverflow_ThenClampInsteadOfThrowing()
    {
        // The old implementation computed Math.Pow(2, n) * 1000 through Convert.ToInt32 and threw an
        // OverflowException at attempt 22, long after the waits had already grown to days.
        var waiter = new JitteredWaiter(maxRetryCount: 100, maxWaitMilliseconds: 20);

        for (var attempt = 0; attempt < 60; attempt++)
        {
            await waiter.WaitAsync();
        }

        Assert.True(waiter.CanRetry);
    }

    [Fact]
    public async Task GivenMaxWaitMilliseconds_WhenBackoffExceedsIt_ThenWaitIsCapped()
    {
        var waiter = new JitteredWaiter(maxRetryCount: 10, maxWaitMilliseconds: 50);

        // Attempt 5 would otherwise wait roughly 16 to 32 seconds.
        for (var attempt = 0; attempt < 5; attempt++)
        {
            await waiter.WaitAsync();
        }

        var stopwatch = Stopwatch.StartNew();

        await waiter.WaitAsync();

        stopwatch.Stop();

        Assert.InRange(stopwatch.ElapsedMilliseconds, 0, 2000);
    }

    [Fact]
    public async Task GivenCancelledToken_WhenWaitAsync_ThenThrowOperationCanceledException()
    {
        var waiter = new JitteredWaiter(5);

        using var cancellation = new CancellationTokenSource();

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiter.WaitAsync(cancellation.Token));
    }

    [Fact]
    public async Task GivenTokenCancelledMidWait_WhenWaitAsync_ThenStopWaitingEarly()
    {
        var waiter = new JitteredWaiter(5);

        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var stopwatch = Stopwatch.StartNew();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiter.WaitAsync(cancellation.Token));

        stopwatch.Stop();

        Assert.InRange(stopwatch.ElapsedMilliseconds, 0, 400);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void GivenNegativeMaxRetryCount_WhenConstructed_ThenThrowArgumentOutOfRangeException(int maxRetryCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new JitteredWaiter(maxRetryCount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNonPositiveMaxWait_WhenConstructed_ThenThrowArgumentOutOfRangeException(int maxWaitMilliseconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new JitteredWaiter(5, maxWaitMilliseconds));
    }
}
