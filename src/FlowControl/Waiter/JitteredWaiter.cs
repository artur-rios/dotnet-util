namespace ArturRios.Util.FlowControl.Waiter;

/// <summary>
/// Implements an exponential backoff waiting strategy with jitter to reduce contention.
/// </summary>
/// <remarks>
/// Wait times grow exponentially (2^n seconds base minus a fixed delay) and a random jitter is added.
/// Call <see cref="Wait"/> before each retry attempt until <see cref="CanRetry"/> is false.
/// </remarks>
/// <param name="maxRetryCount">Maximum number of retries allowed.</param>
public class JitteredWaiter(int maxRetryCount)
{
    private const int FixedWaitDelay = 500;

    /// <summary>
    /// Maximum number of retries permitted.
    /// </summary>
    public int MaxRetryCount { get; } = maxRetryCount;

    private int Count { get; set; }

    /// <summary>
    /// Indicates whether another retry attempt can be performed.
    /// </summary>
    public bool CanRetry => Count < MaxRetryCount;

    /// <summary>
    /// Asynchronously waits based on the current retry attempt using exponential backoff with jitter.
    /// </summary>
    /// <exception cref="MaxRetriesReachedException">Thrown when called more times than <see cref="MaxRetryCount"/>.</exception>
    public async Task Wait()
    {
        if (Count >= MaxRetryCount)
        {
            throw new MaxRetriesReachedException();
        }

        var currentRetryAttempt = Count++;

        if (currentRetryAttempt == 0)
        {
            await Task.Delay(FixedWaitDelay);
        }
        else
        {
            var backoffPeriodMs = Convert.ToInt32(Math.Pow(2, currentRetryAttempt) * 1000) - FixedWaitDelay;
            await Task.Delay(FixedWaitDelay + backoffPeriodMs / 2 + new System.Random().Next(0, backoffPeriodMs / 2));
        }
    }
}
