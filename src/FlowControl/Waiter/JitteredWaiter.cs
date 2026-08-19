namespace ArturRios.Util.FlowControl.Waiter;

/// <summary>
/// Implements an exponential backoff waiting strategy with jitter to reduce contention.
/// </summary>
/// <remarks>
/// Wait times grow exponentially (2^n seconds base minus a fixed delay) and a random jitter is added.
/// Call <see cref="WaitAsync"/> before each retry attempt until <see cref="CanRetry"/> is false.
/// Every wait is clamped to <see cref="MaxWaitMilliseconds"/>, so a large
/// <paramref name="maxRetryCount"/> produces a long series of capped waits rather than an arithmetic
/// overflow or a multi-day sleep.
/// </remarks>
/// <param name="maxRetryCount">Maximum number of retries allowed; must not be negative.</param>
/// <param name="maxWaitMilliseconds">
/// Upper bound applied to a single wait, in milliseconds. Defaults to
/// <see cref="DefaultMaxWaitMilliseconds"/> (30 seconds); must be greater than zero.
/// </param>
/// <exception cref="ArgumentOutOfRangeException">
/// <paramref name="maxRetryCount"/> is negative, or <paramref name="maxWaitMilliseconds"/> is not positive.
/// </exception>
public class JitteredWaiter(int maxRetryCount, int maxWaitMilliseconds = JitteredWaiter.DefaultMaxWaitMilliseconds)
{
    /// <summary>
    /// Default ceiling for a single wait: 30 seconds.
    /// </summary>
    public const int DefaultMaxWaitMilliseconds = 30_000;

    private readonly Lock _lock = new();

    /// <summary>
    /// Maximum number of retries permitted.
    /// </summary>
    public int MaxRetryCount { get; } = maxRetryCount >= 0
        ? maxRetryCount
        : throw new ArgumentOutOfRangeException(nameof(maxRetryCount), maxRetryCount, "Value must not be negative.");

    /// <summary>
    /// Ceiling applied to a single wait, in milliseconds.
    /// </summary>
    public int MaxWaitMilliseconds { get; } = maxWaitMilliseconds > 0
        ? maxWaitMilliseconds
        : throw new ArgumentOutOfRangeException(nameof(maxWaitMilliseconds), maxWaitMilliseconds, "Value must be greater than zero.");

    private int Count { get; set; }

    /// <summary>
    /// Indicates whether another retry attempt can be performed.
    /// </summary>
    public bool CanRetry
    {
        get
        {
            lock (_lock)
            {
                return Count < MaxRetryCount;
            }
        }
    }

    /// <summary>
    /// Asynchronously waits based on the current retry attempt using exponential backoff with jitter.
    /// </summary>
    /// <param name="cancellationToken">Cancels the wait.</param>
    /// <exception cref="MaxRetriesReachedException">Thrown when called more times than <see cref="MaxRetryCount"/>.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        int currentRetryAttempt;

        lock (_lock)
        {
            if (Count >= MaxRetryCount)
            {
                throw new MaxRetriesReachedException();
            }

            currentRetryAttempt = Count++;
        }

        await Task.Delay(Backoff.DelayFor(currentRetryAttempt, MaxWaitMilliseconds), cancellationToken).ConfigureAwait(false);
    }
}
