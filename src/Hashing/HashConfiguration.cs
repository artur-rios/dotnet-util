namespace ArturRios.Util.Hashing;

/// <summary>
/// Configuration values for controlling Argon2id hashing cost parameters.
/// </summary>
/// <remarks>
/// The defaults are deliberately expensive: a single hash reserves
/// <see cref="DefaultMemoryToUseInKb"/> kilobytes (roughly 600 MB) across
/// <see cref="DefaultDegreeOfParallelism"/> lanes. That is appropriate for a login path that runs rarely,
/// but a server hashing many secrets concurrently will exhaust its memory long before its CPU, so size the
/// parameters against the expected concurrency. Note that changing them invalidates existing hashes:
/// a stored hash can only be verified with the configuration it was produced with.
/// </remarks>
/// <param name="degreeOfParallelism">Number of threads (lanes) to use; defaults to <see cref="DefaultDegreeOfParallelism"/>. Recommended value is number of CPU cores x 2.</param>
/// <param name="numberOfIterations">Number of iterations; defaults to <see cref="DefaultNumberOfIterations"/>.</param>
/// <param name="memoryToUseInKb">Memory size in kilobytes; defaults to <see cref="DefaultMemoryToUseInKb"/>.</param>
/// <exception cref="ArgumentOutOfRangeException">Any supplied value is less than one.</exception>
public class HashConfiguration(
    int? degreeOfParallelism = null,
    int? numberOfIterations = null,
    int? memoryToUseInKb = null)
{
    /// <summary>
    /// Default parallelism (threads) used when unspecified.
    /// </summary>
    public const int DefaultDegreeOfParallelism = 16;

    /// <summary>
    /// Default recommended minimum iteration count.
    /// </summary>
    public const int DefaultNumberOfIterations = 4;

    /// <summary>
    /// Default memory cost (600 MB) expressed in kilobytes.
    /// </summary>
    public const int DefaultMemoryToUseInKb = 600000;

    /// <summary>
    /// Gets the configured degree of parallelism.
    /// </summary>
    public int DegreeOfParallelism { get; } =
        Positive(degreeOfParallelism ?? DefaultDegreeOfParallelism, nameof(degreeOfParallelism));

    /// <summary>
    /// Gets the configured number of iterations.
    /// </summary>
    public int NumberOfIterations { get; } =
        Positive(numberOfIterations ?? DefaultNumberOfIterations, nameof(numberOfIterations));

    /// <summary>
    /// Gets the configured memory usage in kilobytes.
    /// </summary>
    public int MemoryToUseInKb { get; } =
        Positive(memoryToUseInKb ?? DefaultMemoryToUseInKb, nameof(memoryToUseInKb));

    /// <summary>
    /// Rejects cost parameters Argon2 cannot honour.
    /// </summary>
    private static int Positive(int value, string parameterName)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 1, parameterName);

        return value;
    }
}
