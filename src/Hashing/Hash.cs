using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace ArturRios.Util.Hashing;

/// <summary>
/// Provides hashing helpers based on Argon2id for secure password / secret derivation.
/// </summary>
/// <remarks>
/// All methods use a key length of 128 bytes and allow optional configuration overrides via <see cref="HashConfiguration"/>.
/// </remarks>
public static class Hash
{
    private const int Argon2IdKeyBytes = 128;
    private const int SaltByteSize = 16;

    /// <summary>
    /// Hashes <paramref name="text"/> using Argon2id with a provided salt and optional configuration.
    /// </summary>
    /// <param name="text">The input text to hash (e.g. password).</param>
    /// <param name="salt">A cryptographically strong random salt.</param>
    /// <param name="configuration">Optional hashing configuration; defaults are used if null.</param>
    /// <returns>The derived hash bytes.</returns>
    public static byte[] EncodeWithSalt(string text, byte[] salt, HashConfiguration? configuration = null)
    {
        configuration ??= new HashConfiguration();

        Argon2id argon2Id = new(Encoding.UTF8.GetBytes(text))
        {
            Salt = salt,
            DegreeOfParallelism = configuration.DegreeOfParallelism,
            Iterations = configuration.NumberOfIterations,
            MemorySize = configuration.MemoryToUseInKb
        };

        return argon2Id.GetBytes(Argon2IdKeyBytes);
    }

    /// <summary>
    /// Hashes <paramref name="text"/> using Argon2id and a newly generated random 16-byte salt.
    /// </summary>
    /// <param name="text">The input text to hash.</param>
    /// <param name="salt">Outputs the generated salt used for hashing.</param>
    /// <param name="configuration">Optional hashing configuration.</param>
    /// <returns>The derived hash bytes.</returns>
    public static byte[] EncodeWithRandomSalt(string text, out byte[] salt, HashConfiguration? configuration = null)
    {
        configuration ??= new HashConfiguration();
        salt = CreateSalt();

        Argon2id argon2Id = new(Encoding.UTF8.GetBytes(text))
        {
            Salt = salt,
            DegreeOfParallelism = configuration.DegreeOfParallelism,
            Iterations = configuration.NumberOfIterations,
            MemorySize = configuration.MemoryToUseInKb
        };

        return argon2Id.GetBytes(Argon2IdKeyBytes);
    }

    /// <summary>
    /// Verifies if the provided <paramref name="hash"/> matches hashing <paramref name="text"/> with <paramref name="salt"/>.
    /// </summary>
    /// <param name="text">Plain text to verify.</param>
    /// <param name="hash">Expected hash value.</param>
    /// <param name="salt">Salt associated with the stored hash.</param>
    /// <returns><c>true</c> if the hashes match; otherwise <c>false</c>.</returns>
    public static bool TextMatches(string text, byte[] hash, byte[] salt)
    {
        var hashToMatch = EncodeWithSalt(text, salt);

        return hash.SequenceEqual(hashToMatch);
    }

    /// <summary>
    /// Creates a cryptographically strong random salt.
    /// </summary>
    /// <returns>A 16-byte salt.</returns>
    private static byte[] CreateSalt()
    {
        var buffer = new byte[SaltByteSize];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(buffer);

        return buffer;
    }
}
