using System.Diagnostics;
using ArturRios.Util.FlowControl.Waiter;

namespace ArturRios.Util.Tests.FlowControl.Waiter;

/// <summary>
/// The waiter's bookkeeping and its use of the schedule. The schedule itself is checked without sleeping in
/// <see cref="BackoffTests"/>, so these tests keep every real wait to a millisecond or two by setting a low
/// ceiling, except where the length of the wait is what they prove.
/// </summary>
[Trait("Category", "Unit")]
public class JitteredWaiterTests
{
    // A ceiling that turns every wait into a token one, for the tests that are about counting.
    private const int TokenWaitMilliseconds = 1;

    [Fact]
    public void GivenNoCeiling_WhenConstructed_ThenUseTheDefaultCeiling()
    {
        var waiter = new JitteredWaiter(3);

        Assert.Equal(3, waiter.MaxRetryCount);
        Assert.Equal(JitteredWaiter.DefaultMaxWaitMilliseconds, waiter.MaxWaitMilliseconds);
        Assert.Equal(30_000, JitteredWaiter.DefaultMaxWaitMilliseconds);
    }

    [Fact]
    public void GivenACeiling_WhenConstructed_ThenKeepIt()
    {
        Assert.Equal(250, new JitteredWaiter(3, 250).MaxWaitMilliseconds);
    }

    [Fact]
    public async Task GivenACeilingBelowTheFirstWait_WhenWaitAsync_ThenWaitTheCeiling()
    {
        // The first wait is the fixed 500 ms, capped here to 80 ms: long enough to measure, short enough to wait.
        const int ceiling = 80;

        var waiter = new JitteredWaiter(maxRetryCount: 1, maxWaitMilliseconds: ceiling);

        var stopwatch = Stopwatch.StartNew();

        await waiter.WaitAsync(TestContext.Current.CancellationToken);

        stopwatch.Stop();

        // Timers may fire up to a clock tick early; nothing bounds how late a loaded machine runs them.
        Assert.True(stopwatch.ElapsedMilliseconds >= ceiling - 20, $"waited only {stopwatch.ElapsedMilliseconds} ms");
    }

    [Fact]
    public async Task GivenMaxRetryCount_WhenWaitAsync_ThenUpdateCanRetryPropertyCorrectly()
    {
        var waiter = new JitteredWaiter(2, TokenWaitMilliseconds);

        Assert.True(waiter.CanRetry);

        await waiter.WaitAsync(TestContext.Current.CancellationToken);

        Assert.True(waiter.CanRetry);

        await waiter.WaitAsync(TestContext.Current.CancellationToken);

        Assert.False(waiter.CanRetry);
    }

    [Fact]
    public async Task GivenMaxRetryCount_WhenExceedingRetries_ThenThrowMaxRetriesReachedException()
    {
        var waiter = new JitteredWaiter(2, TokenWaitMilliseconds);

        await waiter.WaitAsync(TestContext.Current.CancellationToken);
        await waiter.WaitAsync(TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<MaxRetriesReachedException>(() =>
            waiter.WaitAsync(TestContext.Current.CancellationToken));

        Assert.Equal(["Maximum retry count exceeded"], exception.Messages);
        Assert.False(waiter.CanRetry);
    }

    [Fact]
    public async Task GivenZeroMaxRetryCount_WhenWaitAsync_ThenThrowImmediately()
    {
        var waiter = new JitteredWaiter(0);

        Assert.False(waiter.CanRetry);

        await Assert.ThrowsAsync<MaxRetriesReachedException>(() => waiter.WaitAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenHighRetryCount_WhenAttemptIndexWouldOverflow_ThenClampInsteadOfThrowing()
    {
        // The old implementation computed Math.Pow(2, n) * 1000 through Convert.ToInt32 and threw an
        // OverflowException at attempt 22, long after the waits had already grown to days.
        var waiter = new JitteredWaiter(maxRetryCount: 100, maxWaitMilliseconds: TokenWaitMilliseconds);

        for (var attempt = 0; attempt < 60; attempt++)
        {
            await waiter.WaitAsync(TestContext.Current.CancellationToken);
        }

        Assert.True(waiter.CanRetry);
    }

    [Fact]
    public async Task GivenConcurrentCallers_WhenWaitAsync_ThenGrantExactlyMaxRetryCountWaits()
    {
        const int maxRetryCount = 20;

        var waiter = new JitteredWaiter(maxRetryCount, TokenWaitMilliseconds);

        var calls = Enumerable.Range(0, maxRetryCount * 3)
            .Select(_ => Task.Run(async () =>
            {
                try
                {
                    await waiter.WaitAsync(TestContext.Current.CancellationToken);

                    return true;
                }
                catch (MaxRetriesReachedException)
                {
                    return false;
                }
            }, TestContext.Current.CancellationToken));

        var granted = await Task.WhenAll(calls);

        Assert.Equal(maxRetryCount, granted.Count(wasGranted => wasGranted));
        Assert.False(waiter.CanRetry);
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
        // The first wait lasts 500 ms. Had it run to the end, WaitAsync would have returned normally instead of
        // throwing, so the exception alone proves the wait was cut short; no clock is needed.
        var waiter = new JitteredWaiter(5);

        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiter.WaitAsync(cancellation.Token));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void GivenNegativeMaxRetryCount_WhenConstructed_ThenThrowArgumentOutOfRangeException(int maxRetryCount)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new JitteredWaiter(maxRetryCount));

        Assert.Equal("maxRetryCount", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNonPositiveMaxWait_WhenConstructed_ThenThrowArgumentOutOfRangeException(int maxWaitMilliseconds)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new JitteredWaiter(5, maxWaitMilliseconds));

        Assert.Equal("maxWaitMilliseconds", exception.ParamName);
    }

    [Fact]
    public void GivenCustomMessages_WhenMaxRetriesReachedExceptionIsCreated_ThenCarryThemInsteadOfTheDefault()
    {
        var exception = new MaxRetriesReachedException(["gave up on the payment gateway"]);

        Assert.Equal(["gave up on the payment gateway"], exception.Messages);
        Assert.Equal("gave up on the payment gateway", exception.Message);
    }
}
