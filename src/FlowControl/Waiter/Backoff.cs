namespace ArturRios.Util.FlowControl.Waiter;

/// <summary>
/// The exponential-backoff-with-jitter schedule shared by <see cref="JitteredWaiter"/> and
/// <see cref="Retry"/>, so the two cannot drift apart.
/// </summary>
internal static class Backoff
{
    /// <summary>
    /// Wait applied before the first retry, and the floor every later wait builds on.
    /// </summary>
    internal const int FixedWaitDelay = 500;

    /// <summary>
    /// Computes the delay for a zero-based retry attempt: half the backoff period plus a random half.
    /// </summary>
    /// <param name="attempt">The zero-based retry attempt.</param>
    /// <param name="maxWaitMilliseconds">Ceiling applied to the result.</param>
    /// <remarks>
    /// The exponent is computed in 64-bit arithmetic and clamped before it is narrowed, so an attempt
    /// index large enough to overflow a 32-bit millisecond count simply saturates at
    /// <paramref name="maxWaitMilliseconds"/> instead of throwing.
    /// </remarks>
    internal static int DelayFor(int attempt, int maxWaitMilliseconds)
    {
        if (attempt <= 0)
        {
            return System.Math.Min(FixedWaitDelay, maxWaitMilliseconds);
        }

        // 2^52 milliseconds already dwarfs any sane ceiling, so shifting further would only overflow.
        var backoffPeriodMs = attempt >= 52
            ? long.MaxValue / 2L
            : (1L << attempt) * 1000L - FixedWaitDelay;

        var half = backoffPeriodMs / 2L;
        var jitter = half > 0L ? System.Random.Shared.NextInt64(0L, half) : 0L;

        return (int)System.Math.Min(FixedWaitDelay + half + jitter, maxWaitMilliseconds);
    }
}
