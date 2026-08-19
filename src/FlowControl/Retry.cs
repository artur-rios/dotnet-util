using ArturRios.Util.FlowControl.Waiter;

namespace ArturRios.Util.FlowControl;

/// <summary>
/// Provides a simple retry mechanism for executing actions or functions with optional fixed delay between attempts.
/// </summary>
/// <remarks>
/// <para>
/// Configure using <see cref="MaxAttempts"/>, <see cref="DelayMilliseconds"/> and <see cref="When"/>, then call
/// one of the <c>Execute</c> or <c>ExecuteAsync</c> overloads. The last exception is rethrown once the attempts
/// are exhausted.
/// </para>
/// <para>
/// <see cref="MaxAttempts"/> counts total executions, not additional ones: <c>MaxAttempts(3)</c> runs the
/// operation at most three times, so at most two retries follow the first attempt.
/// </para>
/// <para>
/// A configured instance is immutable during execution and may be reused and shared across threads. The
/// fluent setters mutate the instance, so finish configuring before handing it to another thread.
/// </para>
/// </remarks>
public class Retry
{
    private Func<Exception, bool> _shouldRetry = _ => true;
    private int _delayMilliseconds;
    private int _maxAttempts = 1;
    private int? _backoffCeilingMilliseconds;

    /// <summary>
    /// Creates a new <see cref="Retry"/> instance. Syntactic sugar for <c>new Retry()</c>.
    /// </summary>
    public static Retry New => new();

    /// <summary>
    /// Sets the total number of times the operation may run before the last exception is rethrown.
    /// </summary>
    /// <param name="maxAttempts">Total number of executions; must be greater than zero. Defaults to 1, meaning no retry.</param>
    /// <returns>The configured <see cref="Retry"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxAttempts"/> is less than one.</exception>
    public Retry MaxAttempts(int maxAttempts)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);

        _maxAttempts = maxAttempts;

        return this;
    }

    /// <summary>
    /// Sets a fixed delay (in milliseconds) to wait after a failed attempt before retrying.
    /// </summary>
    /// <param name="delayMilliseconds">Delay duration in milliseconds; must not be negative.</param>
    /// <returns>The configured <see cref="Retry"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="delayMilliseconds"/> is negative.</exception>
    /// <remarks>Mutually exclusive with <see cref="JitteredBackoff"/>; the last call configured wins.</remarks>
    public Retry DelayMilliseconds(int delayMilliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(delayMilliseconds);

        _delayMilliseconds = delayMilliseconds;
        _backoffCeilingMilliseconds = null;

        return this;
    }

    /// <summary>
    /// Waits with exponential backoff and jitter between attempts instead of a fixed delay, using the same
    /// schedule as <see cref="JitteredWaiter"/>.
    /// </summary>
    /// <param name="maxWaitMilliseconds">
    /// Ceiling applied to a single wait. Defaults to <see cref="JitteredWaiter.DefaultMaxWaitMilliseconds"/>
    /// (30 seconds); must be greater than zero.
    /// </param>
    /// <returns>The configured <see cref="Retry"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxWaitMilliseconds"/> is not positive.</exception>
    /// <remarks>
    /// Prefer this over <see cref="DelayMilliseconds"/> when several callers retry the same failing
    /// dependency: a fixed delay synchronizes them into repeated thundering herds.
    /// </remarks>
    public Retry JitteredBackoff(int maxWaitMilliseconds = JitteredWaiter.DefaultMaxWaitMilliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxWaitMilliseconds, 1);

        _backoffCeilingMilliseconds = maxWaitMilliseconds;
        _delayMilliseconds = 0;

        return this;
    }

    /// <summary>
    /// Restricts retrying to the exceptions matched by <paramref name="predicate"/>. Anything else is
    /// rethrown on the first attempt.
    /// </summary>
    /// <param name="predicate">Returns true for exceptions worth retrying.</param>
    /// <returns>The configured <see cref="Retry"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    /// <remarks>
    /// By default every exception is retried except <see cref="OperationCanceledException"/>, which always
    /// propagates immediately: retrying a canceled operation defeats the cancellation.
    /// </remarks>
    public Retry When(Func<Exception, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _shouldRetry = predicate;

        return this;
    }

    /// <summary>
    /// Restricts retrying to exceptions assignable to <typeparamref name="TException"/>.
    /// </summary>
    /// <typeparam name="TException">The exception type worth retrying.</typeparam>
    /// <returns>The configured <see cref="Retry"/> instance.</returns>
    public Retry When<TException>() where TException : Exception => When(exception => exception is TException);

    /// <summary>
    /// Executes an <see cref="Action"/> applying the configured retry strategy.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    /// <exception cref="Exception">Rethrows the last exception encountered after all attempts fail.</exception>
    public void Execute(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        Execute(() =>
        {
            action();

            return true;
        });
    }

    /// <summary>
    /// Executes a function applying the configured retry strategy and returns its result.
    /// </summary>
    /// <typeparam name="T">Return type of the function.</typeparam>
    /// <param name="func">The function to invoke.</param>
    /// <returns>The value returned by <paramref name="func"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <c>null</c>.</exception>
    /// <exception cref="Exception">Rethrows the last exception encountered after all attempts fail.</exception>
    public T Execute<T>(Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var attemptsLeft = _maxAttempts;
        var attempt = 0;

        while (true)
        {
            try
            {
                return func();
            }
            catch (Exception exception)
            {
                attemptsLeft--;

                if (attemptsLeft <= 0 || !IsWorthRetrying(exception))
                {
                    throw;
                }

                var delay = DelayBefore(attempt++);

                if (delay > 0)
                {
                    Thread.Sleep(delay);
                }
            }
        }
    }

    /// <summary>
    /// Executes an asynchronous operation applying the configured retry strategy.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancels the delay between attempts and prevents further attempts.</param>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
    /// <exception cref="Exception">Rethrows the last exception encountered after all attempts fail.</exception>
    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await ExecuteAsync(async token =>
        {
            await operation(token).ConfigureAwait(false);

            return true;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an asynchronous operation applying the configured retry strategy and returns its result.
    /// </summary>
    /// <typeparam name="T">Return type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancels the delay between attempts and prevents further attempts.</param>
    /// <returns>The value returned by <paramref name="operation"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
    /// <exception cref="Exception">Rethrows the last exception encountered after all attempts fail.</exception>
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var attemptsLeft = _maxAttempts;
        var attempt = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                attemptsLeft--;

                if (attemptsLeft <= 0 || !IsWorthRetrying(exception))
                {
                    throw;
                }

                var delay = DelayBefore(attempt++);

                if (delay > 0)
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Returns how long to wait before the retry that follows a zero-based failed attempt.
    /// </summary>
    private int DelayBefore(int attempt) =>
        _backoffCeilingMilliseconds is { } ceiling ? Backoff.DelayFor(attempt, ceiling) : _delayMilliseconds;

    /// <summary>
    /// Decides whether an exception should trigger another attempt.
    /// </summary>
    /// <remarks>
    /// Cancellation is never retried, whatever the configured predicate says.
    /// </remarks>
    private bool IsWorthRetrying(Exception exception) =>
        exception is not OperationCanceledException && _shouldRetry(exception);
}
