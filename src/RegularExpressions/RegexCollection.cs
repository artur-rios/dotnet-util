using System.Text.RegularExpressions;

namespace ArturRios.Util.RegularExpressions;

/// <summary>
/// Collection of commonly used regular expressions with source-generated compiled variants.
/// </summary>
public static partial class RegexCollection
{
    /// <summary>
    /// Pattern that matches RFC 5322-like email addresses (simplified).
    /// </summary>
    public const string EmailPattern =
        @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";

    /// <summary>Pattern that checks if a string contains at least one digit.</summary>
    public const string HasNumberPattern = "[0-9]+";
    /// <summary>Pattern that checks if a string contains at least one lowercase character.</summary>
    public const string HasLowerCharPattern = "[a-z]+";
    /// <summary>Pattern that checks if a string contains at least one uppercase character.</summary>
    public const string HasUpperCharPattern = "[A-Z]+";
    /// <summary>Pattern that validates a string contains at least one lowercase, one uppercase and one digit.</summary>
    public const string HasNumberLowerAndUpperCharPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$";

    /// <summary>
    /// Returns a compiled regex for <see cref="EmailPattern"/>.
    /// </summary>
    [GeneratedRegex(EmailPattern)]
    public static partial Regex Email();

    /// <summary>
    /// Returns a compiled regex for <see cref="HasNumberPattern"/>.
    /// </summary>
    [GeneratedRegex(HasNumberPattern)]
    public static partial Regex HasNumber();

    /// <summary>
    /// Returns a compiled regex for <see cref="HasLowerCharPattern"/>.
    /// </summary>
    [GeneratedRegex(HasLowerCharPattern)]
    public static partial Regex HasLowerChar();

    /// <summary>
    /// Returns a compiled regex for <see cref="HasUpperCharPattern"/>.
    /// </summary>
    [GeneratedRegex(HasUpperCharPattern)]
    public static partial Regex HasUpperChar();

    /// <summary>
    /// Returns a compiled regex for <see cref="HasNumberLowerAndUpperCharPattern"/>.
    /// </summary>
    [GeneratedRegex(HasNumberLowerAndUpperCharPattern)]
    public static partial Regex HasNumberLowerAndUpperChar();
}
