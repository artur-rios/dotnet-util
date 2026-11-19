using ArturRios.Util.FlowControl.Waiter;

namespace ArturRios.Util.Tests.FlowControl.Waiter;

/// <summary>
/// The schedule <see cref="JitteredWaiter"/> and <see cref="ArturRios.Util.FlowControl.Retry"/> wait by, checked
/// by computing it rather than by sleeping through it.
/// </summary>
[Trait("Category", "Unit")]
public class BackoffTests
{
    // Enough draws for the jitter to reach both ends of its range many times over.
    private const int Samples = 2000;

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void GivenTheFirstAttempt_WhenDelayFor_ThenWaitTheFixedDelay(int attempt)
    {
        Assert.Equal(Backoff.FixedWaitDelay, Backoff.DelayFor(attempt, JitteredWaiter.DefaultMaxWaitMilliseconds));
    }

    [Fact]
    public void GivenTheFirstAttemptAndALowerCeiling_WhenDelayFor_ThenWaitTheCeiling()
    {
        Assert.Equal(100, Backoff.DelayFor(0, 100));
    }

    [Theory]
    [InlineData(1, 1250, 2000)]
    [InlineData(2, 2250, 4000)]
    [InlineData(3, 4250, 8000)]
    [InlineData(4, 8250, 16000)]
    public void GivenALaterAttempt_WhenDelayFor_ThenWaitHalfTheBackoffPeriodPlusARandomHalf(int attempt,
        int inclusiveMinimum, int exclusiveMaximum)
    {
        // The period is 2^attempt seconds minus the fixed delay; the wait is the fixed delay, half the period
        // and a jitter drawn from the other half.
        var delays = Enumerable.Range(0, Samples)
            .Select(_ => Backoff.DelayFor(attempt, JitteredWaiter.DefaultMaxWaitMilliseconds))
            .ToArray();

        Assert.All(delays, delay => Assert.InRange(delay, inclusiveMinimum, exclusiveMaximum - 1));
    }

    [Fact]
    public void GivenALaterAttempt_WhenDelayForRepeatedly_ThenTheJitterVariesTheWait()
    {
        var delays = Enumerable.Range(0, Samples)
            .Select(_ => Backoff.DelayFor(3, JitteredWaiter.DefaultMaxWaitMilliseconds))
            .Distinct()
            .Count();

        Assert.True(delays > 1, "every draw produced the same delay");
    }

    [Fact]
    public void GivenConsecutiveAttempts_WhenDelayFor_ThenEveryWaitIsLongerThanAnyWaitBeforeIt()
    {
        for (var attempt = 0; attempt < 14; attempt++)
        {
            var longestNow = Enumerable.Range(0, Samples).Max(_ => Backoff.DelayFor(attempt, int.MaxValue));
            var shortestNext = Enumerable.Range(0, Samples).Min(_ => Backoff.DelayFor(attempt + 1, int.MaxValue));

            Assert.True(shortestNext > longestNow, $"attempt {attempt + 1} waited {shortestNext} ms, attempt {attempt} {longestNow} ms");
        }
    }

    [Theory]
    [InlineData(5, 50)]
    [InlineData(5, 16_000)]
    [InlineData(20, 30_000)]
    public void GivenABackoffLongerThanTheCeiling_WhenDelayFor_ThenWaitTheCeiling(int attempt, int ceiling)
    {
        // Attempt 5 would wait 16.25 to 32 seconds, attempt 20 several days.
        Assert.Equal(ceiling, Backoff.DelayFor(attempt, ceiling));
    }

    [Theory]
    [InlineData(23)]
    [InlineData(51)]
    [InlineData(52)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(int.MaxValue)]
    public void GivenAnAttemptWhoseBackoffWouldOverflow_WhenDelayFor_ThenSaturateAtTheCeiling(int attempt)
    {
        // From attempt 23 on even the shortest wait exceeds int.MaxValue milliseconds, and a 64-bit shift wraps at
        // 64; neither may throw or wrap around to a short wait.
        Assert.Equal(JitteredWaiter.DefaultMaxWaitMilliseconds,
            Backoff.DelayFor(attempt, JitteredWaiter.DefaultMaxWaitMilliseconds));
        Assert.Equal(int.MaxValue, Backoff.DelayFor(attempt, int.MaxValue));
    }
}
