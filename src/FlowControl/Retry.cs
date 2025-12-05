namespace ArturRios.Util.FlowControl;

/// <summary>
/// Provides a simple retry mechanism for executing actions or functions with optional fixed delay between attempts.
/// </summary>
/// <remarks>
/// Configure using <see cref="MaxAttempts"/> and <see cref="DelayMilliseconds"/> then call <see cref="Execute(Action)"/> or <see cref="Execute{T}(Func{T})"/>.
/// Exceptions are rethrown after the maximum number of attempts has been exhausted.
/// </remarks>
public class Retry
{
    private int _delayMilliseconds;
    private int _maxAttempts;

    /// <summary>
    /// Creates a new <see cref="Retry"/> instance. Syntactic sugar for <c>new Retry()</c>.
    /// </summary>
    public static Retry New => new();

    /// <summary>
    /// Sets the maximum number of retry attempts before giving up and rethrowing the last exception.
    /// </summary>
    /// <param name="maxAttempts">Number of attempts; must be greater than zero.</param>
    /// <returns>The configured <see cref="Retry"/> instance.</returns>
    public Retry MaxAttempts(int maxAttempts)
    {
        _maxAttempts = maxAttempts;

        return this;
    }

    /// <summary>
    /// Sets a fixed delay (in milliseconds) to wait after a failed attempt before retrying.
    /// </summary>
    /// <param name="delayMilliseconds">Delay duration in milliseconds.</param>
    /// <returns>The configured <see cref="Retry"/> instance.</returns>
    public Retry DelayMilliseconds(int delayMilliseconds)
    {
        _delayMilliseconds = delayMilliseconds;

        return this;
    }

    /// <summary>
    /// Executes an <see cref="Action"/> applying the configured retry strategy.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <exception cref="Exception">Rethrows the last exception encountered after all retries fail.</exception>
    public void Execute(Action action)
    {
        while (true)
        {
            try
            {
                action();
                break;
            }
            catch
            {
                if (_maxAttempts-- <= 0)
                {
                    throw;
                }

                if (_delayMilliseconds > 0)
                {
                    Thread.Sleep(_delayMilliseconds);
                }
            }
        }
    }

    /// <summary>
    /// Executes a function applying the configured retry strategy and returns its result.
    /// </summary>
    /// <typeparam name="T">Return type of the function.</typeparam>
    /// <param name="func">The function to invoke.</param>
    /// <returns>The value returned by <paramref name="func"/>.</returns>
    /// <exception cref="Exception">Rethrows the last exception encountered after all retries fail.</exception>
    public T Execute<T>(Func<T> func)
    {
        while (true)
        {
            try
            {
                return func();
            }
            catch
            {
                if (_maxAttempts-- <= 0)
                {
                    throw;
                }

                if (_delayMilliseconds > 0)
                {
                    Thread.Sleep(_delayMilliseconds);
                }
            }
        }
    }
}
