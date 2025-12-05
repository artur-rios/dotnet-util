using System.Diagnostics;
using ArturRios.Util.FlowControl.Waiter;

namespace ArturRios.Util.Tests.FlowControl;

public class JitteredWaiterTests
{
    [Theory]
    [InlineData(0, 500, 500)]
    [InlineData(1, 1250, 1999)]
    [InlineData(2, 2250, 3999)]
    public async Task Should_WaitWithinExpectedRangeForAttempt(int attemptIndex, int expectedMinMs, int expectedMaxMs)
    {
        var waiter = new JitteredWaiter(maxRetryCount: 10);
        
        for (var i = 0; i < attemptIndex; i++)
        {
            await waiter.Wait();
        }

        var sw = Stopwatch.StartNew();
        await waiter.Wait();
        
        sw.Stop();

        var elapsed = sw.ElapsedMilliseconds;
        
        var lowerBound = Math.Max(0, expectedMinMs - 150); 
        var upperBound = expectedMaxMs + 350;

        Assert.InRange(elapsed, lowerBound, upperBound);
    }

    [Fact]
    public async Task Should_UpdateCanRetryPropertyCorrectly()
    {
        var waiter = new JitteredWaiter(2);

        Assert.True(waiter.CanRetry);
        
        await waiter.Wait();
        
        Assert.True(waiter.CanRetry);
        
        await waiter.Wait();
        
        Assert.False(waiter.CanRetry);
    }

    [Fact]
    public async Task Should_ThrowExceptionAfterExceedingMaxRetryCount()
    {
        var waiter = new JitteredWaiter(2);
        
        await waiter.Wait();
        await waiter.Wait();
        
        await Assert.ThrowsAsync<MaxRetriesReachedException>(() => waiter.Wait());
    }

    [Fact]
    public async Task Should_ThrowImmediatelyWhenMaxRetryCountIsZero()
    {
        var waiter = new JitteredWaiter(0);
        
        Assert.False(waiter.CanRetry);
        
        await Assert.ThrowsAsync<MaxRetriesReachedException>(() => waiter.Wait());
    }
}