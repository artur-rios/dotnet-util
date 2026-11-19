using ArturRios.Util.FlowControl;
using ArturRios.Util.Tests.Setup;

namespace ArturRios.Util.Tests.FlowControl;

[Trait("Category", "Unit")]
public class RetryTests
{
    private int _attemptCount;

    [Fact]
    public void GivenFailingOperation_WhenRetryWithResult_ThenReturnFinalAttempt()
    {
        const int maxAttempts = 3;
        const int delayMilliseconds = 10;

        var result = Retry.New
            .MaxAttempts(maxAttempts)
            .DelayMilliseconds(delayMilliseconds)
            .Execute(() => TestMethod(maxAttempts));

        Assert.Equal(maxAttempts, result);
    }

    [Fact]
    public void GivenFailingVoidOperation_WhenRetryVoidMethod_ThenNoException()
    {
        const int maxAttempts = 3;
        const int delayMilliseconds = 10;

        var retry = Retry.New
            .MaxAttempts(maxAttempts)
            .DelayMilliseconds(delayMilliseconds);

        var exception = Record.Exception(() => retry.Execute(() => VoidTestMethod(maxAttempts)));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void GivenAlwaysFailingOperation_WhenExecute_ThenRunExactlyMaxAttemptsTimes(int maxAttempts)
    {
        var executions = 0;

        Assert.Throws<InvalidOperationException>(() => Retry.New
            .MaxAttempts(maxAttempts)
            .Execute(() =>
            {
                executions++;

                throw new InvalidOperationException("always fails");
            }));

        // MaxAttempts counts total executions, so MaxAttempts(3) means one attempt plus two retries.
        Assert.Equal(maxAttempts, executions);
    }

    [Fact]
    public void GivenReusedInstance_WhenExecutedTwice_ThenSecondRunGetsTheSameAttemptBudget()
    {
        // The former implementation decremented the configured field, leaving a used instance with
        // zero attempts left.
        var retry = Retry.New.MaxAttempts(3);

        var firstRunExecutions = 0;
        var secondRunExecutions = 0;

        Assert.Throws<InvalidOperationException>(() => retry.Execute(() =>
        {
            firstRunExecutions++;

            throw new InvalidOperationException("always fails");
        }));

        Assert.Throws<InvalidOperationException>(() => retry.Execute(() =>
        {
            secondRunExecutions++;

            throw new InvalidOperationException("always fails");
        }));

        Assert.Equal(3, firstRunExecutions);
        Assert.Equal(firstRunExecutions, secondRunExecutions);
    }

    [Fact]
    public void GivenUnconfiguredInstance_WhenExecute_ThenRunOnceAndRethrow()
    {
        var executions = 0;

        Assert.Throws<InvalidOperationException>(() => Retry.New.Execute(() =>
        {
            executions++;

            throw new InvalidOperationException("always fails");
        }));

        Assert.Equal(1, executions);
    }

    [Fact]
    public void GivenSucceedingOperation_WhenExecute_ThenRunOnlyOnce()
    {
        var executions = 0;

        var result = Retry.New.MaxAttempts(5).Execute(() =>
        {
            executions++;

            return 42;
        });

        Assert.Equal(42, result);
        Assert.Equal(1, executions);
    }

    [Fact]
    public async Task GivenConfiguredDelay_WhenRetrying_ThenWaitBetweenAttempts()
    {
        const int delayMilliseconds = 120;

        var clock = new RecordingTimeProvider();
        var executions = 0;

        var run = Task.Run(() => Assert.Throws<InvalidOperationException>(() => Retry.New
            .MaxAttempts(3)
            .DelayMilliseconds(delayMilliseconds)
            .UseTimeProvider(clock)
            .Execute(() =>
            {
                Interlocked.Increment(ref executions);

                throw new InvalidOperationException("always fails");
            })), TestContext.Current.CancellationToken);

        // The second attempt waits for the clock to reach the delay, and not a millisecond less.
        await clock.NextTimerAsync(TestContext.Current.CancellationToken);
        clock.Advance(TimeSpan.FromMilliseconds(delayMilliseconds - 1));

        Assert.Equal(1, Volatile.Read(ref executions));

        clock.Advance(TimeSpan.FromMilliseconds(1));
        await clock.AdvanceThroughNextTimerAsync(TestContext.Current.CancellationToken);
        await run;

        // Three attempts means two delays.
        Assert.Equal(3, executions);
        Assert.Equal([TimeSpan.FromMilliseconds(delayMilliseconds), TimeSpan.FromMilliseconds(delayMilliseconds)],
            clock.DueTimes);
    }

    [Fact]
    public async Task GivenConfiguredDelay_WhenExecuteAsyncRetries_ThenWaitExactlyTheDelayBetweenAttempts()
    {
        const int delayMilliseconds = 120;

        var clock = new RecordingTimeProvider();
        var executions = 0;

        var run = Assert.ThrowsAsync<InvalidOperationException>(() => Retry.New
            .MaxAttempts(3)
            .DelayMilliseconds(delayMilliseconds)
            .UseTimeProvider(clock)
            .ExecuteAsync(_ =>
            {
                Interlocked.Increment(ref executions);

                throw new InvalidOperationException("always fails");
            }, TestContext.Current.CancellationToken));

        await clock.NextTimerAsync(TestContext.Current.CancellationToken);
        clock.Advance(TimeSpan.FromMilliseconds(delayMilliseconds - 1));

        Assert.Equal(1, Volatile.Read(ref executions));

        clock.Advance(TimeSpan.FromMilliseconds(1));
        await clock.AdvanceThroughNextTimerAsync(TestContext.Current.CancellationToken);
        await run;

        Assert.Equal(3, executions);
        Assert.Equal([TimeSpan.FromMilliseconds(delayMilliseconds), TimeSpan.FromMilliseconds(delayMilliseconds)],
            clock.DueTimes);
    }

    [Fact]
    public async Task GivenTheDefaultBackoffAndAFakeClock_WhenExecuteAsyncRetries_ThenEachDelayFollowsTheSchedule()
    {
        // Several seconds of backoff at the default ceiling, timed by the clock in no real time.
        var clock = new RecordingTimeProvider();

        var run = Assert.ThrowsAsync<InvalidOperationException>(() => Retry.New
            .MaxAttempts(4)
            .JitteredBackoff()
            .UseTimeProvider(clock)
            .ExecuteAsync(_ => throw new InvalidOperationException("always fails"), TestContext.Current.CancellationToken));

        for (var delay = 0; delay < 3; delay++)
        {
            await clock.AdvanceThroughNextTimerAsync(TestContext.Current.CancellationToken);
        }

        await run;

        var delays = clock.DueTimes.Select(dueTime => dueTime.TotalMilliseconds).ToArray();

        Assert.Equal(3, delays.Length);
        Assert.Equal(500, delays[0]);
        Assert.InRange(delays[1], 1250, 1999);
        Assert.InRange(delays[2], 2250, 3999);
    }

    [Fact]
    public async Task GivenAFakeClock_WhenTheTokenIsCancelledMidDelay_ThenStopWaitingAndNeverRetry()
    {
        var clock = new RecordingTimeProvider();
        var executions = 0;

        using var cancellation = new CancellationTokenSource();

        var run = Assert.ThrowsAnyAsync<OperationCanceledException>(() => Retry.New
            .MaxAttempts(3)
            .DelayMilliseconds(5000)
            .UseTimeProvider(clock)
            .ExecuteAsync(_ =>
            {
                Interlocked.Increment(ref executions);

                throw new InvalidOperationException("always fails");
            }, cancellation.Token));

        // The clock stops a millisecond short of the delay: only the cancellation can end it.
        await clock.NextTimerAsync(TestContext.Current.CancellationToken);
        clock.Advance(TimeSpan.FromMilliseconds(4999));
        await cancellation.CancelAsync();
        await run;

        Assert.Equal(1, executions);
    }

    [Fact]
    public void GivenANullTimeProvider_WhenConfigured_ThenThrowArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Retry.New.UseTimeProvider(null!));

        Assert.Equal("timeProvider", exception.ParamName);
    }

    [Fact]
    public void GivenExceptionPredicate_WhenExceptionDoesNotMatch_ThenRethrowWithoutRetrying()
    {
        var executions = 0;

        Assert.Throws<InvalidOperationException>(() => Retry.New
            .MaxAttempts(5)
            .When<TimeoutException>()
            .Execute(() =>
            {
                executions++;

                throw new InvalidOperationException("not retryable");
            }));

        Assert.Equal(1, executions);
    }

    [Fact]
    public void GivenExceptionPredicate_WhenExceptionMatches_ThenRetry()
    {
        var executions = 0;

        Assert.Throws<TimeoutException>(() => Retry.New
            .MaxAttempts(4)
            .When<TimeoutException>()
            .Execute(() =>
            {
                executions++;

                throw new TimeoutException("retryable");
            }));

        Assert.Equal(4, executions);
    }

    [Fact]
    public void GivenCancelledOperation_WhenExecute_ThenNeverRetry()
    {
        var executions = 0;

        Assert.Throws<OperationCanceledException>(() => Retry.New
            .MaxAttempts(5)
            .Execute(() =>
            {
                executions++;

                throw new OperationCanceledException();
            }));

        Assert.Equal(1, executions);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNonPositiveMaxAttempts_WhenConfigured_ThenThrowArgumentOutOfRangeException(int maxAttempts)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Retry.New.MaxAttempts(maxAttempts));
    }

    [Fact]
    public void GivenNegativeDelay_WhenConfigured_ThenThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Retry.New.DelayMilliseconds(-1));
    }

    [Fact]
    public void GivenNullOperation_WhenExecute_ThenThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Retry.New.Execute((Action)null!));
        Assert.Throws<ArgumentNullException>(() => Retry.New.Execute((Func<int>)null!));
    }

    [Fact]
    public void GivenNullPredicate_WhenConfigured_ThenThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Retry.New.When(null!));
    }

    [Fact]
    public async Task GivenFailingAsyncOperation_WhenExecuteAsync_ThenRetryUntilItSucceeds()
    {
        var executions = 0;

        var result = await Retry.New
            .MaxAttempts(3)
            .DelayMilliseconds(10)
            .ExecuteAsync(_ =>
            {
                executions++;

                return executions < 3
                    ? throw new InvalidOperationException("not yet")
                    : Task.FromResult(executions);
            }, TestContext.Current.CancellationToken);

        Assert.Equal(3, result);
        Assert.Equal(3, executions);
    }

    [Fact]
    public async Task GivenAlwaysFailingAsyncOperation_WhenExecuteAsync_ThenRunExactlyMaxAttemptsTimes()
    {
        var executions = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Retry.New
            .MaxAttempts(4)
            .ExecuteAsync(_ =>
            {
                executions++;

                throw new InvalidOperationException("always fails");
            }, TestContext.Current.CancellationToken));

        Assert.Equal(4, executions);
    }

    [Fact]
    public async Task GivenCancelledToken_WhenExecuteAsync_ThenThrowWithoutRunningTheOperation()
    {
        var executions = 0;

        using var cancellation = new CancellationTokenSource();

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Retry.New
            .MaxAttempts(5)
            .ExecuteAsync(_ =>
            {
                executions++;

                return Task.CompletedTask;
            }, cancellation.Token));

        Assert.Equal(0, executions);
    }

    [Fact]
    public async Task GivenTokenCancelledBetweenAttempts_WhenExecuteAsync_ThenStopRetrying()
    {
        var executions = 0;

        using var cancellation = new CancellationTokenSource();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Retry.New
            .MaxAttempts(10)
            .DelayMilliseconds(50)
            .ExecuteAsync(async _ =>
            {
                executions++;

                if (executions == 2)
                {
                    await cancellation.CancelAsync();
                }

                throw new InvalidOperationException("always fails");
            }, cancellation.Token));

        Assert.Equal(2, executions);
    }

    [Fact]
    public async Task GivenNullAsyncOperation_WhenExecuteAsync_ThenThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            Retry.New.ExecuteAsync((Func<CancellationToken, Task>)null!, TestContext.Current.CancellationToken));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            Retry.New.ExecuteAsync((Func<CancellationToken, Task<int>>)null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenJitteredBackoff_WhenRetrying_ThenWaitTheCappedBackoffBetweenAttempts()
    {
        // The schedule itself is checked without sleeping in BackoffTests; this proves Execute sleeps by it.
        const int ceiling = 60;

        var clock = new RecordingTimeProvider();

        var run = Task.Run(() => Assert.Throws<InvalidOperationException>(() => Retry.New
            .MaxAttempts(3)
            .JitteredBackoff(maxWaitMilliseconds: ceiling)
            .UseTimeProvider(clock)
            .Execute(() => throw new InvalidOperationException("always fails"))), TestContext.Current.CancellationToken);

        await clock.AdvanceThroughNextTimerAsync(TestContext.Current.CancellationToken);
        await clock.AdvanceThroughNextTimerAsync(TestContext.Current.CancellationToken);
        await run;

        // Three attempts, two waits; both backoff waits exceed the ceiling, so each is the ceiling.
        Assert.Equal([TimeSpan.FromMilliseconds(ceiling), TimeSpan.FromMilliseconds(ceiling)], clock.DueTimes);
    }

    [Fact]
    public async Task GivenJitteredBackoff_WhenExecuteAsync_ThenWaitTheCappedBackoffBetweenAttempts()
    {
        const int ceiling = 60;

        var clock = new RecordingTimeProvider();
        var executions = 0;

        var run = Assert.ThrowsAsync<InvalidOperationException>(() => Retry.New
            .MaxAttempts(3)
            .JitteredBackoff(maxWaitMilliseconds: ceiling)
            .UseTimeProvider(clock)
            .ExecuteAsync(_ =>
            {
                Interlocked.Increment(ref executions);

                throw new InvalidOperationException("always fails");
            }, TestContext.Current.CancellationToken));

        await clock.AdvanceThroughNextTimerAsync(TestContext.Current.CancellationToken);
        await clock.AdvanceThroughNextTimerAsync(TestContext.Current.CancellationToken);
        await run;

        Assert.Equal(3, executions);
        Assert.Equal([TimeSpan.FromMilliseconds(ceiling), TimeSpan.FromMilliseconds(ceiling)], clock.DueTimes);
    }

    [Fact]
    public async Task GivenJitteredBackoffThenFixedDelay_WhenConfigured_ThenTheLastCallWins()
    {
        var clock = new RecordingTimeProvider();

        // Run apart and bounded, so that a wait the clock is never advanced through fails the test instead of
        // hanging it.
        await Task.Run(() => Assert.Throws<InvalidOperationException>(() => Retry.New
                .MaxAttempts(3)
                .JitteredBackoff(maxWaitMilliseconds: 5000)
                .DelayMilliseconds(0)
                .UseTimeProvider(clock)
                .Execute(() => throw new InvalidOperationException("always fails"))), TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        // The backoff would have waited 500 ms and then at least 1250 ms; the fixed delay of zero replaced it.
        Assert.Empty(clock.DueTimes);
    }

    [Fact]
    public async Task GivenFixedDelayThenJitteredBackoff_WhenConfigured_ThenTheLastCallWins()
    {
        const int ceiling = 60;

        var clock = new RecordingTimeProvider();

        var run = Task.Run(() => Assert.Throws<InvalidOperationException>(() => Retry.New
            .MaxAttempts(2)
            .DelayMilliseconds(5000)
            .JitteredBackoff(maxWaitMilliseconds: ceiling)
            .UseTimeProvider(clock)
            .Execute(() => throw new InvalidOperationException("always fails"))), TestContext.Current.CancellationToken);

        await clock.AdvanceThroughNextTimerAsync(TestContext.Current.CancellationToken);
        await run;

        // The fixed delay would have waited 5 s; the backoff, capped at the ceiling, replaced it.
        Assert.Equal([TimeSpan.FromMilliseconds(ceiling)], clock.DueTimes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNonPositiveBackoffCeiling_WhenConfigured_ThenThrowArgumentOutOfRangeException(int ceiling)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Retry.New.JitteredBackoff(ceiling));
    }

    [Fact]
    public void GivenTheFluentSetters_WhenCalled_ThenReturnTheSameInstance()
    {
        var retry = Retry.New;

        Assert.Same(retry, retry.MaxAttempts(2));
        Assert.Same(retry, retry.DelayMilliseconds(0));
        Assert.Same(retry, retry.JitteredBackoff());
        Assert.Same(retry, retry.When(_ => true));
        Assert.Same(retry, retry.When<TimeoutException>());
        Assert.Same(retry, retry.UseTimeProvider(TimeProvider.System));
        Assert.NotSame(Retry.New, Retry.New);
    }

    [Fact]
    public async Task GivenFailingVoidAsyncOperation_WhenExecuteAsync_ThenRetryUntilItSucceeds()
    {
        var executions = 0;

        await Retry.New
            .MaxAttempts(3)
            .ExecuteAsync(_ =>
            {
                executions++;

                return executions < 3 ? throw new InvalidOperationException("not yet") : Task.CompletedTask;
            }, TestContext.Current.CancellationToken);

        Assert.Equal(3, executions);
    }

    [Fact]
    public async Task GivenToken_WhenExecuteAsync_ThenPassItToTheOperation()
    {
        using var cancellation = new CancellationTokenSource();

        CancellationToken received = default;

        await Retry.New.ExecuteAsync(token =>
        {
            received = token;

            return Task.CompletedTask;
        }, cancellation.Token);

        Assert.Equal(cancellation.Token, received);
    }

    [Fact]
    public async Task GivenExceptionPredicate_WhenExecuteAsyncFailsWithAnUnmatchedException_ThenRethrowWithoutRetrying()
    {
        var executions = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Retry.New
            .MaxAttempts(5)
            .When(exception => exception is TimeoutException)
            .ExecuteAsync(_ =>
            {
                executions++;

                throw new InvalidOperationException("not retryable");
            }, TestContext.Current.CancellationToken));

        Assert.Equal(1, executions);
    }

    [Fact]
    public async Task GivenOperationCancellingItself_WhenExecuteAsync_ThenNeverRetryEvenIfThePredicateWouldMatch()
    {
        var executions = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Retry.New
            .MaxAttempts(5)
            .When(_ => true)
            .ExecuteAsync(_ =>
            {
                executions++;

                throw new TaskCanceledException();
            }, TestContext.Current.CancellationToken));

        Assert.Equal(1, executions);
    }

    [Fact]
    public async Task GivenOneInstanceSharedByConcurrentCallers_WhenExecuteAsync_ThenEachCallGetsTheFullAttemptBudget()
    {
        var retry = Retry.New.MaxAttempts(3);

        var executions = new int[8];

        var calls = Enumerable.Range(0, executions.Length).Select(caller => Task.Run(() =>
            Assert.ThrowsAsync<InvalidOperationException>(() => retry.ExecuteAsync<int>(async _ =>
            {
                Interlocked.Increment(ref executions[caller]);

                await Task.Yield();

                throw new InvalidOperationException("always fails");
            }, TestContext.Current.CancellationToken))));

        await Task.WhenAll(calls);

        Assert.All(executions, count => Assert.Equal(3, count));
    }

    private int TestMethod(int maxAttempts)
    {
        _attemptCount++;

        return _attemptCount < maxAttempts ? throw new Exception("Simulated failure") : _attemptCount;
    }

    private void VoidTestMethod(int maxAttempts)
    {
        _attemptCount++;

        if (_attemptCount < maxAttempts)
        {
            throw new Exception("Simulated failure");
        }
    }
}
