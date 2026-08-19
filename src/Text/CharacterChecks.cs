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

    /// <summary>
    /// Reports every <see cref="CharacterClasses"/> present in <paramref name="value"/> in a single pass.
    /// </summary>
    /// <param name="value">Input text.</param>
    /// <returns>The classes present, or <see cref="CharacterClasses.None"/> when none are.</returns>
    /// <remarks>
    /// Unlike <see cref="RegularExpressions.RegexCollection.HasNumberLowerAndUpperCharPattern"/>, which walks
    /// the input once per lookahead and rejects anything spanning more than one line, this reads the input
    /// once, stops as soon as every class has been seen, and is indifferent to newlines.
    /// </remarks>
    public static CharacterClasses Classify(this ReadOnlySpan<char> value)
    {
        const CharacterClasses everything = CharacterClasses.Digit | CharacterClasses.Lower |
                                            CharacterClasses.Upper | CharacterClasses.Special;
        var found = CharacterClasses.None;

        foreach (var character in value)
        {
            if (character is >= '0' and <= '9')
            {
                found |= CharacterClasses.Digit;
            }
            else if (character is >= 'a' and <= 'z')
            {
                found |= CharacterClasses.Lower;
            }
            else if (character is >= 'A' and <= 'Z')
            {
                found |= CharacterClasses.Upper;
            }
            else if (SpecialChars.Contains(character))
            {
                found |= CharacterClasses.Special;
            }

            if (found == everything)
            {
                return everything;
            }
        }

        return found;
    }

    /// <summary>
    /// Reports every <see cref="CharacterClasses"/> present in <paramref name="value"/> in a single pass.
    /// </summary>
    /// <param name="value">Input text. A <see langword="null"/> value contains nothing.</param>
    /// <returns>The classes present, or <see cref="CharacterClasses.None"/> when none are.</returns>
    public static CharacterClasses Classify(this string? value) => value.AsSpan().Classify();

    /// <summary>
    /// Reports which of the <paramref name="required"/> classes <paramref name="value"/> does not contain.
    /// </summary>
    /// <param name="value">Input text.</param>
    /// <param name="required">The classes the caller demands.</param>
    /// <returns>
    /// The required classes that are absent, or <see cref="CharacterClasses.None"/> when every requirement
    /// is met. Classes present but not required are never reported.
    /// </returns>
    public static CharacterClasses Missing(this ReadOnlySpan<char> value, CharacterClasses required) =>
        required & ~value.Classify();

    /// <summary>
    /// Reports which of the <paramref name="required"/> classes <paramref name="value"/> does not contain.
    /// </summary>
    /// <param name="value">Input text. A <see langword="null"/> value is missing every requirement.</param>
    /// <param name="required">The classes the caller demands.</param>
    /// <returns>
    /// The required classes that are absent, or <see cref="CharacterClasses.None"/> when every requirement
    /// is met. Classes present but not required are never reported.
    /// </returns>
    public static CharacterClasses Missing(this string? value, CharacterClasses required) =>
        value.AsSpan().Missing(required);
}
