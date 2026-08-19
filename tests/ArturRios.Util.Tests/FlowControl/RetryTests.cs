using System.Diagnostics;
using ArturRios.Util.FlowControl;

namespace ArturRios.Util.Tests.FlowControl;

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
    public void GivenConfiguredDelay_WhenRetrying_ThenWaitBetweenAttempts()
    {
        const int delayMilliseconds = 120;

        var stopwatch = Stopwatch.StartNew();

        Assert.Throws<InvalidOperationException>(() => Retry.New
            .MaxAttempts(3)
            .DelayMilliseconds(delayMilliseconds)
            .Execute(() => throw new InvalidOperationException("always fails")));

        stopwatch.Stop();

        // Three attempts means two delays; allow generous slack for scheduling.
        Assert.InRange(stopwatch.ElapsedMilliseconds, delayMilliseconds * 2 - 40, delayMilliseconds * 2 + 3000);
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
            });

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
            }));

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
            Retry.New.ExecuteAsync((Func<CancellationToken, Task>)null!));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            Retry.New.ExecuteAsync((Func<CancellationToken, Task<int>>)null!));
    }

    [Fact]
    public void GivenJitteredBackoff_WhenRetrying_ThenWaitAndGrowBetweenAttempts()
    {
        var stopwatch = Stopwatch.StartNew();

        Assert.Throws<InvalidOperationException>(() => Retry.New
            .MaxAttempts(3)
            .JitteredBackoff(maxWaitMilliseconds: 200)
            .Execute(() => throw new InvalidOperationException("always fails")));

        stopwatch.Stop();

        // Two waits, each capped at 200 ms and each at least a few milliseconds.
        Assert.InRange(stopwatch.ElapsedMilliseconds, 20, 3000);
    }

    [Fact]
    public async Task GivenJitteredBackoff_WhenExecuteAsync_ThenWaitBetweenAttempts()
    {
        var executions = 0;

        var stopwatch = Stopwatch.StartNew();

        await Assert.ThrowsAsync<InvalidOperationException>(() => Retry.New
            .MaxAttempts(3)
            .JitteredBackoff(maxWaitMilliseconds: 150)
            .ExecuteAsync(_ =>
            {
                executions++;

                throw new InvalidOperationException("always fails");
            }));

        stopwatch.Stop();

        Assert.Equal(3, executions);
        Assert.InRange(stopwatch.ElapsedMilliseconds, 20, 3000);
    }

    [Fact]
    public void GivenJitteredBackoffThenFixedDelay_WhenConfigured_ThenTheLastCallWins()
    {
        var stopwatch = Stopwatch.StartNew();

        Assert.Throws<InvalidOperationException>(() => Retry.New
            .MaxAttempts(2)
            .JitteredBackoff(maxWaitMilliseconds: 5000)
            .DelayMilliseconds(0)
            .Execute(() => throw new InvalidOperationException("always fails")));

        stopwatch.Stop();

        // The backoff would have waited at least 500 ms; the fixed delay of zero replaced it.
        Assert.InRange(stopwatch.ElapsedMilliseconds, 0, 400);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNonPositiveBackoffCeiling_WhenConfigured_ThenThrowArgumentOutOfRangeException(int ceiling)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Retry.New.JitteredBackoff(ceiling));
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
