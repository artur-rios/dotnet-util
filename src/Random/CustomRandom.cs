using System.Security.Cryptography;
using System.Text;
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
    /// Generates a random string respecting the constraints defined in <paramref name="options"/>.
    /// </summary>
    /// <param name="options">Options controlling length and character inclusion.</param>
    /// <param name="differentFrom">Optional collection of strings to exclude from results.</param>
    /// <returns>A randomly generated string.</returns>
    public static string Text(RandomStringOptions options, string[]? differentFrom = null)
    {
        while (true)
        {
            var random = new System.Random();
            var password = new StringBuilder();

            var charCollectionCount = 0;

            if (options.IncludeLowercase)
            {
                password.Append(Characters.LowerLetters[random.Next(Characters.LowerLetters.Length)]);
                charCollectionCount++;
            }

            if (options.IncludeUppercase)
            {
                password.Append(Characters.UpperLetters[random.Next(Characters.UpperLetters.Length)]);
                charCollectionCount++;
            }

            if (options.IncludeSpecialCharacters)
            {
                password.Append(Characters.Special[random.Next(Characters.Special.Length)]);
                charCollectionCount++;
            }

            if (options.IncludeDigits)
            {
                password.Append(Characters.Digits[random.Next(Characters.Digits.Length)]);
                charCollectionCount++;
            }

            for (var i = charCollectionCount; i < options.Length; i++)
            {
                password.Append(Characters.All[random.Next(Characters.All.Length)]);
            }

            var generatedString = new string(password.ToString().OrderBy(_ => random.Next()).ToArray());
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
