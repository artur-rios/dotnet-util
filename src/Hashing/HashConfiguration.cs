namespace ArturRios.Util.Hashing;

/// <summary>
/// Configuration values for controlling Argon2id hashing cost parameters.
/// </summary>
/// <param name="degreeOfParallelism">Number of threads (lanes) to use; defaults to <see cref="DefaultDegreeOfParallelism"/>. Recommended value is number of CPU cores x 2.</param>
/// <param name="numberOfIterations">Number of iterations; defaults to <see cref="DefaultNumberOfIterations"/>.</param>
/// <param name="memoryToUseInKb">Memory size in kilobytes; defaults to <see cref="DefaultMemoryToUseInKb"/>.</param>
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
    public int DegreeOfParallelism { get; } = degreeOfParallelism ?? DefaultDegreeOfParallelism;

    /// <summary>
    /// Gets the configured number of iterations.
    /// </summary>
    public int NumberOfIterations { get; } = numberOfIterations ?? DefaultNumberOfIterations;

    /// <summary>
    /// Gets the configured memory usage in kilobytes.
    /// </summary>
    public int MemoryToUseInKb { get; } = memoryToUseInKb ?? DefaultMemoryToUseInKb;
}
