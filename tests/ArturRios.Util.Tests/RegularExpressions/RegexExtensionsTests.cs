using System.Text.RegularExpressions;
using ArturRios.Util.RegularExpressions;

namespace ArturRios.Util.Tests.RegularExpressions;

public partial class RegexExtensionsTests
{
    [Fact]
    public void GivenInputWithDigits_WhenRemove_ThenReturnStringWithoutDigits()
    {
        const string input = "abc123def45";

        var result = Digits().Remove(input);

        Assert.Equal("abcdef", result);
    }

    [Fact]
    public void GivenInputWithoutMatches_WhenRemove_ThenReturnOriginalString()
    {
        const string input = "abcdef";

        var result = Xyz().Remove(input);

        Assert.Equal(input, result);
    }

    [Fact]
    public void GivenEmptyInput_WhenRemove_ThenReturnEmptyString()
    {
        var input = string.Empty;

        var result = Xyz().Remove(input);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GivenInputWithWhitespace_WhenRemove_ThenReturnStringWithoutWhitespace()
    {
        const string input = "a b\tc \n d";

        var result = WhiteSpace().Remove(input);

        Assert.Equal("abcd", result);
    }

    [Fact]
    public void GivenCaseInsensitiveRegex_WhenRemove_ThenRespectRegexOptions()
    {
        const string input = "FOO bar Foo";

        var result = CaseInsensitiveFoo().Remove(input);

        Assert.Equal(" bar ", result);
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex Digits();

    [GeneratedRegex("xyz")]
    private static partial Regex Xyz();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpace();

    [GeneratedRegex("foo", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex CaseInsensitiveFoo();
}
