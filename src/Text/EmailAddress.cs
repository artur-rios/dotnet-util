using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ArturRios.Util.RegularExpressions;

namespace ArturRios.Util.Text;

/// <summary>
/// Normalization and validation for email addresses, layered on top of
/// <see cref="RegexCollection.EmailPattern"/>.
/// </summary>
/// <remarks>
/// <para>
/// Validating an address happens at three levels, and this type covers the first two. The pattern in
/// <see cref="RegexCollection"/> answers whether the text is shaped like an address. This type additionally
/// normalizes it, so that two spellings of one mailbox compare equal. Whether the mailbox actually exists
/// can only be settled by an MX lookup or a confirmation email.
/// </para>
/// <para>
/// <see cref="System.Net.Mail.MailAddress"/> is deliberately not used here. It is a parser rather than a
/// validator and accepts a great deal the pattern rejects, including "user@-hostname.com",
/// "user@hostname." and display-name forms such as "John Doe &lt;user@host.com&gt;".
/// </para>
/// </remarks>
public static class EmailAddress
{
    private static readonly IdnMapping Idn = new();

    /// <summary>
    /// Validates <paramref name="value"/> and rewrites it into a canonical form.
    /// </summary>
    /// <param name="value">The address to normalize.</param>
    /// <param name="normalized">
    /// The canonical address when the method returns <see langword="true"/>, otherwise <see langword="null"/>.
    /// The domain is lowercased and, when internationalized, converted to its punycode form; the local part
    /// is left exactly as given, because it is case sensitive per RFC 5321.
    /// </param>
    /// <returns><see langword="true"/> when <paramref name="value"/> is a valid address.</returns>
    /// <remarks>
    /// Surrounding whitespace is not trimmed: the syntax gate stays authoritative, so an address with
    /// stray spaces is rejected rather than silently repaired.
    /// </remarks>
    public static bool TryNormalize(string? value, [NotNullWhen(true)] out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        var separator = value.LastIndexOf('@');

        if (separator <= 0 || separator == value.Length - 1)
        {
            return false;
        }

        var localPart = value[..separator];
        var domain = value[(separator + 1)..];
        var candidate = $"{localPart}@{NormalizeDomain(domain)}";

        if (!RegexCollection.Email().IsMatch(candidate))
        {
            return false;
        }

        normalized = candidate;

        return true;
    }

    /// <summary>
    /// Checks whether <paramref name="value"/> is a valid email address.
    /// </summary>
    /// <param name="value">The address to check.</param>
    /// <returns><see langword="true"/> when the address is valid.</returns>
    /// <remarks>
    /// Accepts everything <see cref="RegexCollection.Email"/> accepts, plus mixed-case and
    /// internationalized domains, which are normalized before the pattern is applied.
    /// </remarks>
    public static bool IsValid(string? value) => TryNormalize(value, out _);

    private static string NormalizeDomain(string domain)
    {
        // An address literal such as [192.168.1.1] is not a domain name and must not be punycoded.
        if (domain.StartsWith('['))
        {
            return domain;
        }

        try
        {
            return Idn.GetAscii(domain).ToLowerInvariant();
        }
        catch (ArgumentException)
        {
            // Not a convertible domain; hand it back untouched and let the pattern reject it.
            return domain;
        }
    }
}
