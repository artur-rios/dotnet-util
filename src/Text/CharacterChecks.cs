using System.Buffers;
using ArturRios.Util.Collections;

namespace ArturRios.Util.Text;

/// <summary>
/// Allocation-free character class checks over strings and spans.
/// </summary>
/// <remarks>
/// These are vectorized equivalents of the corresponding patterns in
/// <see cref="RegularExpressions.RegexCollection"/> and agree with them on every input: both are ASCII
/// only. They deliberately do not use <see cref="char.IsDigit(char)"/>, <see cref="char.IsLower(char)"/>
/// or <see cref="char.IsUpper(char)"/>, which are Unicode-aware and would report, for example, the
/// Arabic-Indic digit three as a digit and the German sharp s as a lowercase letter.
/// </remarks>
public static class CharacterChecks
{
    private static readonly SearchValues<char> Digits = SearchValues.Create(Characters.Digits);
    private static readonly SearchValues<char> LowerLetters = SearchValues.Create(Characters.LowerLetters);
    private static readonly SearchValues<char> UpperLetters = SearchValues.Create(Characters.UpperLetters);
    private static readonly SearchValues<char> SpecialChars = SearchValues.Create(Characters.Special);

    /// <summary>Checks whether <paramref name="value"/> contains at least one ASCII digit.</summary>
    /// <param name="value">Input text.</param>
    /// <returns><see langword="true"/> when a digit 0-9 is present.</returns>
    public static bool HasNumber(this ReadOnlySpan<char> value) => value.ContainsAny(Digits);

    /// <summary>Checks whether <paramref name="value"/> contains at least one ASCII lowercase letter.</summary>
    /// <param name="value">Input text.</param>
    /// <returns><see langword="true"/> when a letter a-z is present.</returns>
    public static bool HasLowerChar(this ReadOnlySpan<char> value) => value.ContainsAny(LowerLetters);

    /// <summary>Checks whether <paramref name="value"/> contains at least one ASCII uppercase letter.</summary>
    /// <param name="value">Input text.</param>
    /// <returns><see langword="true"/> when a letter A-Z is present.</returns>
    public static bool HasUpperChar(this ReadOnlySpan<char> value) => value.ContainsAny(UpperLetters);

    /// <summary>Checks whether <paramref name="value"/> contains at least one <see cref="Characters.Special"/> character.</summary>
    /// <param name="value">Input text.</param>
    /// <returns><see langword="true"/> when a special character is present.</returns>
    public static bool HasSpecialChar(this ReadOnlySpan<char> value) => value.ContainsAny(SpecialChars);

    /// <summary>Checks whether <paramref name="value"/> contains at least one ASCII digit.</summary>
    /// <param name="value">Input text. A <see langword="null"/> value contains nothing.</param>
    /// <returns><see langword="true"/> when a digit 0-9 is present.</returns>
    public static bool HasNumber(this string? value) => value.AsSpan().HasNumber();

    /// <summary>Checks whether <paramref name="value"/> contains at least one ASCII lowercase letter.</summary>
    /// <param name="value">Input text. A <see langword="null"/> value contains nothing.</param>
    /// <returns><see langword="true"/> when a letter a-z is present.</returns>
    public static bool HasLowerChar(this string? value) => value.AsSpan().HasLowerChar();

    /// <summary>Checks whether <paramref name="value"/> contains at least one ASCII uppercase letter.</summary>
    /// <param name="value">Input text. A <see langword="null"/> value contains nothing.</param>
    /// <returns><see langword="true"/> when a letter A-Z is present.</returns>
    public static bool HasUpperChar(this string? value) => value.AsSpan().HasUpperChar();

    /// <summary>Checks whether <paramref name="value"/> contains at least one <see cref="Characters.Special"/> character.</summary>
    /// <param name="value">Input text. A <see langword="null"/> value contains nothing.</param>
    /// <returns><see langword="true"/> when a special character is present.</returns>
    public static bool HasSpecialChar(this string? value) => value.AsSpan().HasSpecialChar();
}
