using ArturRios.Util.FlowControl.Waiter;
using ArturRios.Util.Tests.Setup;
using Microsoft.Extensions.Time.Testing;

namespace ArturRios.Util.Tests.FlowControl.Waiter;

/// <summary>
/// The waiter's bookkeeping and its use of the schedule. The schedule itself is checked without sleeping in
/// <see cref="BackoffTests"/>. The tests about how long a wait lasts time it by a fake clock and assert the exact
/// delay; the tests about counting keep every real wait to a millisecond or two by setting a low ceiling.
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
        // The first wait is the fixed 500 ms, capped here to 80 ms.
        const int ceiling = 80;

        var clock = new FakeTimeProvider();
        var waiter = new JitteredWaiter(maxRetryCount: 1, maxWaitMilliseconds: ceiling, clock);

        var wait = waiter.WaitAsync(TestContext.Current.CancellationToken);

        clock.Advance(TimeSpan.FromMilliseconds(ceiling - 1));

        Assert.False(wait.IsCompleted);

        clock.Advance(TimeSpan.FromMilliseconds(1));

        // Bounded, so a wait the clock failed to end fails the test instead of hanging it.
        await wait.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenAFakeClock_WhenWaitAsyncForEveryRetry_ThenEachWaitFollowsTheBackoffSchedule()
    {
        // About a minute of backoff, timed by the clock in no real time. The ranges are BackoffTests' schedule.
        var clock = new RecordingTimeProvider();
        var waiter = new JitteredWaiter(6, JitteredWaiter.DefaultMaxWaitMilliseconds, clock);

        while (waiter.CanRetry)
        {
            var wait = waiter.WaitAsync(TestContext.Current.CancellationToken);

            await clock.AdvanceThroughNextTimerAsync(TestContext.Current.CancellationToken);
            await wait;
        }

        var waits = clock.DueTimes.Select(dueTime => dueTime.TotalMilliseconds).ToArray();

        Assert.Equal(6, waits.Length);
        Assert.Equal(500, waits[0]);
        Assert.InRange(waits[1], 1250, 1999);
        Assert.InRange(waits[2], 2250, 3999);
        Assert.InRange(waits[3], 4250, 7999);
        Assert.InRange(waits[4], 8250, 15999);
        Assert.InRange(waits[5], 16250, JitteredWaiter.DefaultMaxWaitMilliseconds);
    }

    [Fact]
    public void GivenANullTimeProvider_WhenConstructed_ThenThrowArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new JitteredWaiter(3, 250, null!));

        Assert.Equal("timeProvider", exception.ParamName);
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
        // OverflowException at attempt 22, long after the waits had already grown to days. A fake clock keeps the
        // default ceiling, so the waits are seen to saturate at it, in no real time.
        var clock = new RecordingTimeProvider();
        var waiter = new JitteredWaiter(100, JitteredWaiter.DefaultMaxWaitMilliseconds, clock);

        for (var attempt = 0; attempt < 60; attempt++)
        {
            var wait = waiter.WaitAsync(TestContext.Current.CancellationToken);

            clock.Advance(TimeSpan.FromMilliseconds(JitteredWaiter.DefaultMaxWaitMilliseconds));

            await wait;
        }

        Assert.True(waiter.CanRetry);
        Assert.Equal(60, clock.DueTimes.Count);
        Assert.All(clock.DueTimes.Skip(6),
            dueTime => Assert.Equal(TimeSpan.FromMilliseconds(JitteredWaiter.DefaultMaxWaitMilliseconds), dueTime));
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
        // The first wait lasts 500 ms, and the clock never reaches it: only the cancellation can end it.
        var clock = new FakeTimeProvider();
        var waiter = new JitteredWaiter(5, JitteredWaiter.DefaultMaxWaitMilliseconds, clock);

        using var cancellation = new CancellationTokenSource();

        var wait = waiter.WaitAsync(cancellation.Token);

        clock.Advance(TimeSpan.FromMilliseconds(499));

        Assert.False(wait.IsCompleted);

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => wait);
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
