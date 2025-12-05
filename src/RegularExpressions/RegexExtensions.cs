using System.Text.RegularExpressions;

namespace ArturRios.Util.RegularExpressions;

/// <summary>
/// Extension helpers for <see cref="Regex"/> operations.
/// </summary>
public static class RegexExtensions
{
    /// <summary>
    /// Removes all matches of the provided <see cref="Regex"/> from <paramref name="string"/>.
    /// </summary>
    /// <param name="regex">The regex whose matches will be removed.</param>
    /// <param name="string">Input text.</param>
    /// <returns>The input text with all matches replaced by an empty string.</returns>
    public static string Remove(this Regex regex, string @string) => regex.Replace(@string, string.Empty);
}
