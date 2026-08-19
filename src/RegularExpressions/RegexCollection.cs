using System.Text.RegularExpressions;

namespace ArturRios.Util.RegularExpressions;

/// <summary>
/// Collection of commonly used regular expressions with source-generated compiled variants.
/// </summary>
public static partial class RegexCollection
{
    /// <summary>
    /// Pattern that matches a single email address (RFC 5322 addr-spec, simplified).
    /// </summary>
    /// <remarks>
    /// The local part is restricted to ASCII (no SMTPUTF8 internationalized addresses).
    /// Domain labels must start and end with an alphanumeric character and the top-level domain
    /// is alphabetic; an IPv4 address literal is accepted only in its bracketed RFC 5321 form,
    /// with octets bounded to 0-255. This validates syntax only; use
    /// <see cref="System.Net.Mail.MailAddress"/> or an SMTP round-trip when deliverability matters.
    /// </remarks>
    public const string EmailPattern =
        @"^[a-zA-Z0-9!#$%&'*+\-/=?\^_`{|}~]+(\.[a-zA-Z0-9!#$%&'*+\-/=?\^_`{|}~]+)*@" +
        @"((([a-zA-Z0-9](([a-zA-Z0-9\-]{0,61})[a-zA-Z0-9])?\.)+[a-zA-Z]{2,63})" +
        @"|(\[((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\]))$";

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
