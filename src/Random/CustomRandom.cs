using System.Security.Cryptography;
using ArturRios.Util.Collections;

namespace ArturRios.Util.Random;

/// <summary>
/// Provides helpers for generating random numbers and strings with optional exclusion constraints.
/// </summary>
public static class CustomRandom
{
    /// <summary>
    /// Generates a cryptographically strong random integer between <paramref name="start"/> (inclusive) and <paramref name="end"/> (inclusive).
    /// </summary>
    /// <param name="start">Minimum value (inclusive).</param>
    /// <param name="end">Maximum value (inclusive).</param>
    /// <param name="differentFrom">Optional value to avoid returning; regeneration occurs until distinct.</param>
    /// <returns>A random integer in the specified range.</returns>
    public static int NumberFromRng(int start, int end, int? differentFrom = null)
    {
        end++;

        var random = RandomNumberGenerator.GetInt32(start, end);

        if (differentFrom is null)
        {
            return random;
        }

        while (random == differentFrom)
        {
            random = RandomNumberGenerator.GetInt32(start, end);
        }

        return random;
    }

    /// <summary>
    /// Generates a random integer using <see cref="System.Random"/> between <paramref name="start"/> (inclusive) and <paramref name="end"/> (exclusive).
    /// </summary>
    /// <param name="start">Minimum value (inclusive).</param>
    /// <param name="end">Maximum value (exclusive).</param>
    /// <param name="differentFrom">Optional value to avoid returning; regeneration occurs until distinct.</param>
    /// <returns>A random integer in the specified range.</returns>
    public static int NumberFromSystemRandom(int start, int end, int? differentFrom = null)
    {
        System.Random rng = new();

        var random = rng.Next(start, end);

        if (differentFrom is null)
        {
            return random;
        }

        while (random == differentFrom)
        {
            random = rng.Next(start, end);
        }

        return random;
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
    /// <param name="differentFrom">Optional collection of strings to exclude from results.</param>
    /// <returns>A randomly generated string of exactly <see cref="RandomStringOptions.Length"/> characters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// No character set is enabled, or <see cref="RandomStringOptions.Length"/> is smaller than the number of enabled
    /// sets — in which case the length and the at-least-one-of-each guarantee cannot both be honoured.
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

        while (true)
        {
            var characters = new char[options.Length];

            for (var i = 0; i < characterSets.Count; i++)
            {
                characters[i] = characterSets[i][RandomNumberGenerator.GetInt32(characterSets[i].Length)];
            }

            RandomNumberGenerator.GetItems<char>(alphabet, characters.AsSpan(characterSets.Count));
            RandomNumberGenerator.Shuffle<char>(characters);

            var generatedString = new string(characters);
            var matchesExcludedString = false;

            if (differentFrom != null)
            {
                matchesExcludedString = differentFrom.Any(excludedString => excludedString.Equals(generatedString));
            }

            if (matchesExcludedString)
            {
                continue;
            }

            return generatedString;
        }
    }
}
