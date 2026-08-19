using System.Security.Cryptography;
using ArturRios.Util.Collections;

namespace ArturRios.Util.Random;

/// <summary>
/// Provides helpers for generating random numbers and strings with optional exclusion constraints.
/// </summary>
/// <remarks>
/// Both number helpers treat their bounds as inclusive on each end, so <c>(1, 6)</c> models a die
/// whichever source of randomness is chosen. Pick <see cref="NumberFromRng"/> for anything a third party
/// must not be able to predict, and <see cref="NumberFromSystemRandom"/> when throughput matters more
/// than unpredictability.
/// </remarks>
public static class CustomRandom
{
    /// <summary>
    /// Caps how many times <see cref="Text"/> redraws while trying to avoid the excluded strings before
    /// concluding that the exclusion list has swallowed the whole search space.
    /// </summary>
    private const int MaxExclusionAttempts = 1000;

    /// <summary>
    /// Generates a cryptographically strong random integer between <paramref name="start"/> (inclusive) and <paramref name="end"/> (inclusive).
    /// </summary>
    /// <param name="start">Minimum value (inclusive).</param>
    /// <param name="end">Maximum value (inclusive).</param>
    /// <param name="differentFrom">Optional value to avoid returning; regeneration occurs until distinct.</param>
    /// <returns>A random integer in the specified range.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="end"/> is less than <paramref name="start"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="differentFrom"/> is the only value the range can produce, which would leave nothing
    /// to return.
    /// </exception>
    /// <remarks>
    /// The whole 32-bit range is supported, including an <paramref name="end"/> of
    /// <see cref="int.MaxValue"/>: the exclusive bound is computed in 64-bit arithmetic so it cannot wrap.
    /// </remarks>
    public static int NumberFromRng(int start, int end, int? differentFrom = null)
    {
        ValidateRange(start, end, differentFrom);

        var random = NextInclusive(start, end);

        while (random == differentFrom)
        {
            random = NextInclusive(start, end);
        }

        return random;
    }

    /// <summary>
    /// Generates a random integer using <see cref="System.Random"/> between <paramref name="start"/> (inclusive) and <paramref name="end"/> (inclusive).
    /// </summary>
    /// <param name="start">Minimum value (inclusive).</param>
    /// <param name="end">Maximum value (inclusive).</param>
    /// <param name="differentFrom">Optional value to avoid returning; regeneration occurs until distinct.</param>
    /// <returns>A random integer in the specified range.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="end"/> is less than <paramref name="start"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="differentFrom"/> is the only value the range can produce, which would leave nothing
    /// to return.
    /// </exception>
    /// <remarks>
    /// <paramref name="end"/> is inclusive, matching <see cref="NumberFromRng"/>. This is not suitable for
    /// tokens, keys or anything else an adversary must not be able to predict.
    /// </remarks>
    public static int NumberFromSystemRandom(int start, int end, int? differentFrom = null)
    {
        ValidateRange(start, end, differentFrom);

        var random = System.Random.Shared.NextInt64(start, (long)end + 1L);

        while (random == differentFrom)
        {
            random = System.Random.Shared.NextInt64(start, (long)end + 1L);
        }

        return (int)random;
    }

    /// <summary>
    /// Generates a cryptographically strong random string respecting the constraints defined in <paramref name="options"/>.
    /// </summary>
    /// <remarks>
    /// Every character is drawn from the union of the character sets enabled in <paramref name="options"/>, and the
    /// result is guaranteed to contain at least one character from each enabled set. The result is therefore suitable
    /// for security tokens: it is sourced from <see cref="RandomNumberGenerator"/>, not <see cref="System.Random"/>.
    /// </remarks>
    /// <param name="options">Options controlling length and character inclusion.</param>
    /// <param name="differentFrom">Optional collection of strings to exclude from results. Null entries are ignored.</param>
    /// <returns>A randomly generated string of exactly <see cref="RandomStringOptions.Length"/> characters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// No character set is enabled, or <see cref="RandomStringOptions.Length"/> is smaller than the number of enabled
    /// sets — in which case the length and the at-least-one-of-each guarantee cannot both be honored.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="differentFrom"/> excluded every candidate produced in
    /// <see cref="MaxExclusionAttempts"/> draws, which means the exclusion list covers effectively the whole
    /// search space. Widen the alphabet or the length rather than retrying.
    /// </exception>
    public static string Text(RandomStringOptions options, string[]? differentFrom = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        var characterSets = new List<string>(4);

        if (options.IncludeLowercase)
        {
            characterSets.Add(Characters.LowerLetters);
        }

        if (options.IncludeUppercase)
        {
            characterSets.Add(Characters.UpperLetters);
        }

        if (options.IncludeSpecialCharacters)
        {
            characterSets.Add(Characters.Special);
        }

        if (options.IncludeDigits)
        {
            characterSets.Add(Characters.Digits);
        }

        if (characterSets.Count == 0)
        {
            throw new ArgumentException("At least one character set must be included.", nameof(options));
        }

        if (options.Length < characterSets.Count)
        {
            throw new ArgumentException(
                $"Length must be at least {characterSets.Count} to include one character from each enabled character set.",
                nameof(options));
        }

        var alphabet = string.Concat(characterSets);

        for (var attempt = 0; attempt < MaxExclusionAttempts; attempt++)
        {
            var characters = new char[options.Length];

            for (var i = 0; i < characterSets.Count; i++)
            {
                characters[i] = characterSets[i][RandomNumberGenerator.GetInt32(characterSets[i].Length)];
            }

            RandomNumberGenerator.GetItems<char>(alphabet, characters.AsSpan(characterSets.Count));
            RandomNumberGenerator.Shuffle<char>(characters);

            var generatedString = new string(characters);

            if (!IsExcluded(generatedString, differentFrom))
            {
                return generatedString;
            }
        }

        throw new InvalidOperationException(
            $"Could not generate a string outside the excluded set within {MaxExclusionAttempts} attempts. " +
            "The exclusion list covers effectively every string the current options can produce.");
    }

    /// <summary>
    /// Checks a candidate against the exclusion list, tolerating a null entry in it.
    /// </summary>
    private static bool IsExcluded(string candidate, string[]? differentFrom) =>
        differentFrom is not null &&
        Array.Exists(differentFrom, excluded => string.Equals(excluded, candidate, StringComparison.Ordinal));

    /// <summary>
    /// Rejects ranges that are inverted, or that could only ever return the excluded value.
    /// </summary>
    private static void ValidateRange(int start, int end, int? differentFrom)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(end, start);

        if (start == end && differentFrom == start)
        {
            throw new ArgumentException(
                $"The range [{start}, {end}] contains only {differentFrom}, so no different value can be produced.",
                nameof(differentFrom));
        }
    }

    /// <summary>
    /// Draws a uniform value in [<paramref name="start"/>, <paramref name="end"/>] from
    /// <see cref="RandomNumberGenerator"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="RandomNumberGenerator.GetInt32(int, int)"/> takes an exclusive upper bound, which cannot
    /// express an inclusive <see cref="int.MaxValue"/>. That single case is served by rejection sampling
    /// over 64 random bits, which keeps the distribution uniform.
    /// </remarks>
    private static int NextInclusive(int start, int end)
    {
        if (start == end)
        {
            return start;
        }

        if (end < int.MaxValue)
        {
            return RandomNumberGenerator.GetInt32(start, end + 1);
        }

        var range = (ulong)((long)end - start) + 1UL;

        // Values in the incomplete final bucket are rejected, so every bucket stays equally likely.
        var remainder = (ulong.MaxValue % range + 1UL) % range;

        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        ulong draw;

        do
        {
            RandomNumberGenerator.Fill(buffer);

            draw = BitConverter.ToUInt64(buffer);
        }
        while (remainder != 0UL && draw > ulong.MaxValue - remainder);

        return (int)(start + (long)(draw % range));
    }
}
