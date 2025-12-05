namespace ArturRios.Util.Collections;

/// <summary>
/// Provides character set constants used for random string generation and validation.
/// </summary>
/// <remarks>
/// Combine these constants as needed when constructing custom character pools (e.g. password generation).
/// </remarks>
public static class Characters
{
    /// <summary>All decimal digits 0-9.</summary>
    public const string Digits = "0123456789";

    /// <summary>All lowercase ASCII letters a-z.</summary>
    public const string LowerLetters = "abcdefghijklmnopqrstuvwxyz";

    /// <summary>All uppercase ASCII letters A-Z.</summary>
    public const string UpperLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>Common ASCII special characters, excluding whitespace.</summary>
    public const string Special = "!@#$%^&*()_+-=[]{}|;':\",.<>?/";

    /// <summary>Union of <see cref="Digits"/>, <see cref="LowerLetters"/>, <see cref="UpperLetters"/> and <see cref="Special"/>.</summary>
    public const string All = Digits + LowerLetters + UpperLetters + Special;
}
