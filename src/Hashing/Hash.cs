using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace ArturRios.Util.Hashing;

/// <summary>
/// Provides hashing helpers based on Argon2id for secure password / secret derivation.
/// </summary>
/// <remarks>
/// All methods use a key length of 128 bytes and allow optional configuration overrides via <see cref="HashConfiguration"/>.
/// A hash can only be verified with the configuration it was produced with, so store the cost parameters
/// alongside the hash and the salt and pass them back to <see cref="TextMatches"/>.
/// </remarks>
public static class Hash
{
    private const int Argon2IdKeyBytes = 128;
    private const int SaltByteSize = 16;

    /// <summary>
    /// Smallest salt Argon2 accepts. Shorter salts weaken the hash and are rejected outright.
    /// </summary>
    private const int MinimumSaltByteSize = 8;

    /// <summary>
    /// Hashes <paramref name="text"/> using Argon2id with a provided salt and optional configuration.
    /// </summary>
    /// <param name="text">The input text to hash (e.g. password). Must not be empty.</param>
    /// <param name="salt">A cryptographically strong random salt of at least 8 bytes.</param>
    /// <param name="configuration">Optional hashing configuration; defaults are used if null.</param>
    /// <returns>The derived hash bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="salt"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="text"/> is empty, or <paramref name="salt"/> is shorter than 8 bytes.
    /// </exception>
    public static byte[] EncodeWithSalt(string text, byte[] salt, HashConfiguration? configuration = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        ArgumentNullException.ThrowIfNull(salt);

        if (salt.Length < MinimumSaltByteSize)
        {
            throw new ArgumentException($"Salt must be at least {MinimumSaltByteSize} bytes long.", nameof(salt));
        }

        return Derive(text, salt, configuration ?? new HashConfiguration());
    }

    /// <summary>
    /// Hashes <paramref name="text"/> using Argon2id and a newly generated random 16-byte salt.
    /// </summary>
    /// <param name="text">The input text to hash. Must not be empty.</param>
    /// <param name="salt">Outputs the generated salt used for hashing.</param>
    /// <param name="configuration">Optional hashing configuration.</param>
    /// <returns>The derived hash bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="text"/> is empty.</exception>
    /// <remarks>
    /// An empty <paramref name="text"/> is rejected here rather than deep inside Argon2, which reports it
    /// as a null "password" parameter that does not exist in this API.
    /// </remarks>
    public static byte[] EncodeWithRandomSalt(string text, out byte[] salt, HashConfiguration? configuration = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        salt = CreateSalt();

        return Derive(text, salt, configuration ?? new HashConfiguration());
    }

    /// <summary>
    /// Verifies if the provided <paramref name="hash"/> matches hashing <paramref name="text"/> with <paramref name="salt"/>.
    /// </summary>
    /// <param name="text">Plain text to verify.</param>
    /// <param name="hash">Expected hash value.</param>
    /// <param name="salt">Salt associated with the stored hash.</param>
    /// <param name="configuration">
    /// The configuration the stored hash was produced with; defaults are used if null. Passing the wrong
    /// cost parameters produces a different hash and therefore a <c>false</c> result.
    /// </param>
    /// <returns><c>true</c> if the hashes match; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/>, <paramref name="hash"/> or <paramref name="salt"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="text"/> is empty, or <paramref name="salt"/> is shorter than 8 bytes.
    /// </exception>
    /// <remarks>
    /// The comparison runs in time independent of how many leading bytes match, so it does not leak the
    /// correct hash to an attacker who can measure it.
    /// </remarks>
    public static bool TextMatches(string text, byte[] hash, byte[] salt, HashConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(hash);

        var hashToMatch = EncodeWithSalt(text, salt, configuration);

        return CryptographicOperations.FixedTimeEquals(hash, hashToMatch);
    }

    /// <summary>
    /// Runs the Argon2id key derivation, releasing the working memory afterwards.
    /// </summary>
    private static byte[] Derive(string text, byte[] salt, HashConfiguration configuration)
    {
        using Argon2id argon2Id = new(Encoding.UTF8.GetBytes(text))
        {
            Salt = salt,
            DegreeOfParallelism = configuration.DegreeOfParallelism,
            Iterations = configuration.NumberOfIterations,
            MemorySize = configuration.MemoryToUseInKb
        };

        return argon2Id.GetBytes(Argon2IdKeyBytes);
    }

    /// <summary>
    /// Creates a cryptographically strong random salt.
    /// </summary>
    /// <returns>A 16-byte salt.</returns>
    private static byte[] CreateSalt() => RandomNumberGenerator.GetBytes(SaltByteSize);
}
