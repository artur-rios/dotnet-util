using System.Text.RegularExpressions;
using ArturRios.Util.RegularExpressions;

namespace ArturRios.Util.Tests.RegularExpressions;

[Trait("Category", "Unit")]
public class RegexCollectionTests
{
    public static TheoryData<string> InvalidEmails =>
    [
        "-------",
        "@majjf.com",
        "A@b@c@example.com",
        "Abc.example.com",
        "js@proseware..com",
        "ma@@jjf.com",
        "ma@jjf.",
        "ma@jjf..com",
        "ma@jjf.c",
        "ma_@jjf",
        "ma_@jjf.",
        "j@proseware.com9",
        "js@proseware.com9",
        "ma@-hostname.com",
        "ma@hostname-.com",
        "ma@host_name.com",
        "ma@1.2.3.999",
        "ma@192.168.1.1",
        "ma@[1.2.3.999]",
        "joão@hostname.com",
        "mä@hostname.com",
        "中文@hostname.com",
        "ma@hostname.com\n",
        ".ma@hostname.com",
        "ma.@hostname.com",
        "m..a@hostname.com",
        "ma@[256.0.0.1]",
        "ma@[1.2.3]",
        "ma@[01.2.3.4]",
        "ma@hostname.c0m",
        "ma@" + new string('a', 64) + ".com",
        "ma@hostname." + new string('a', 64),
        "ma@hostname.com "
    ];

    public static TheoryData<string> ValidEmails =>
    [
        "ma_@jjf.com",
        "12@hostname.com",
        "d.j@server1.proseware.com",
        "david.jones@proseware.com",
        "j.s@server1.proseware.com",
        "jones@ms1.proseware.com",
        "m.a@hostname.co",
        "m_a1a@hostname.com",
        "ma.h.saraf.onemore@hostname.com.edu",
        "ma@hostname.com",
        "ma12@hostname.com",
        "ma-a.aa@hostname.com.edu",
        "ma-a@hostname.com",
        "ma-a@hostname.com.edu",
        "ma-a@1hostname.com",
        "ma.a@1hostname.com",
        "ma@1hostname.com",
        "ma@hostname.museum",
        "ma@sub.hostname.travel",
        "ma@hostname.comcom",
        "MA@hostname.coMCom",
        "ma@[192.168.1.1]",
        "ma@[10.0.0.255]",
        "ma@[0.0.0.0]",
        "ma@[255.255.255.255]",
        "!#$%&'*+-/=?^_`{|}~@hostname.com",
        "ma@" + new string('a', 63) + ".com",
        "ma@hostname." + new string('a', 63),
        "a@b.co"
    ];

    [Theory]
    [MemberData(nameof(ValidEmails))]
    public void GivenValidEmails_WhenEmailRegex_ThenMatch(string email)
    {
        var result = RegexCollection.Email().IsMatch(email);

        Assert.True(result);
    }


    [Theory]
    [MemberData(nameof(InvalidEmails))]
    public void GivenInvalidEmails_WhenEmailRegex_ThenNotMatch(string email)
    {
        var result = RegexCollection.Email().IsMatch(email);

        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenEmptyOrWhiteSpaceEmails_WhenEmailRegex_ThenNotMatch(string email)
    {
        var result = RegexCollection.Email().IsMatch(email);

        Assert.False(result);
    }

    [Fact]
    public void GivenStringWithNumber_WhenHasNumberRegex_ThenMatch()
    {
        var result = RegexCollection.HasNumber().IsMatch("ABC123");

        Assert.True(result);
    }

    [Fact]
    public void GivenStringWithNoNumber_WhenHasNumberRegex_ThenNotMatch()
    {
        var result = RegexCollection.HasNumber().IsMatch("ABC");

        Assert.False(result);
    }

    [Fact]
    public void GivenStringWithLowerChar_WhenHasLowerCharRegex_ThenMatch()
    {
        var result = RegexCollection.HasLowerChar().IsMatch("ABCabc");

        Assert.True(result);
    }

    [Fact]
    public void GivenStringWithNoLowerChar_WhenHasLowerCharRegex_ThenNotMatch()
    {
        var result = RegexCollection.HasLowerChar().IsMatch("ABC");

        Assert.False(result);
    }

    [Fact]
    public void GivenStringWithUpperChar_WhenHasUpperCharRegex_ThenMatch()
    {
        var result = RegexCollection.HasUpperChar().IsMatch("abcABC");

        Assert.True(result);
    }

    [Fact]
    public void GivenStringWithNoUpperChar_WhenHasUpperCharRegex_ThenNotMatch()
    {
        var result = RegexCollection.HasUpperChar().IsMatch("abc");

        Assert.False(result);
    }

    [Fact]
    public void GivenStringWithNumberLowerAndUpperChar_WhenCompositeRegex_ThenMatch()
    {
        var result = RegexCollection.HasNumberLowerAndUpperChar().IsMatch("abcABC123");

        Assert.True(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("ABC")]
    [InlineData("ABCabc")]
    [InlineData("abc123")]
    [InlineData("ABC123")]
    [InlineData("abcABC123\n")]
    public void GivenStringWithoutNumberLowerAndUpperChar_WhenCompositeRegex_ThenNotMatch(string @string)
    {
        var result = RegexCollection.HasNumberLowerAndUpperChar().IsMatch(@string);

        Assert.False(result);
    }

    [Theory]
    [InlineData(nameof(RegexCollection.Email), RegexCollection.EmailPattern)]
    [InlineData(nameof(RegexCollection.HasNumber), RegexCollection.HasNumberPattern)]
    [InlineData(nameof(RegexCollection.HasLowerChar), RegexCollection.HasLowerCharPattern)]
    [InlineData(nameof(RegexCollection.HasUpperChar), RegexCollection.HasUpperCharPattern)]
    [InlineData(nameof(RegexCollection.HasNumberLowerAndUpperChar), RegexCollection.HasNumberLowerAndUpperCharPattern)]
    public void GivenEveryRegex_WhenInspected_ThenItIsBuiltFromItsPatternWithTheMatchTimeout(string name, string pattern)
    {
        var regex = name switch
        {
            nameof(RegexCollection.Email) => RegexCollection.Email(),
            nameof(RegexCollection.HasNumber) => RegexCollection.HasNumber(),
            nameof(RegexCollection.HasLowerChar) => RegexCollection.HasLowerChar(),
            nameof(RegexCollection.HasUpperChar) => RegexCollection.HasUpperChar(),
            _ => RegexCollection.HasNumberLowerAndUpperChar()
        };

        Assert.Equal(pattern, regex.ToString());
        Assert.Equal(RegexOptions.None, regex.Options);
        Assert.Equal(TimeSpan.FromMilliseconds(RegexCollection.MatchTimeoutMilliseconds), regex.MatchTimeout);
        Assert.Equal(100, RegexCollection.MatchTimeoutMilliseconds);
    }

    [Theory]
    [InlineData("aB1")]
    [InlineData("1aB")]
    [InlineData("pass Word 9")]
    [InlineData("abcABC\u0663")] // \d is Unicode-aware, as the pattern's documentation says
    public void GivenStringWithEveryRequiredClass_WhenCompositeRegex_ThenMatch(string value)
    {
        Assert.Matches(RegexCollection.HasNumberLowerAndUpperChar(), value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("aB\n1")]
    [InlineData("aB1\n")]
    public void GivenEmptyOrMultiLineString_WhenCompositeRegex_ThenNotMatch(string value)
    {
        Assert.DoesNotMatch(RegexCollection.HasNumberLowerAndUpperChar(), value);
    }

    [Theory]
    [InlineData("\u0663")]
    [InlineData("\u00e9")]
    [InlineData("\u00c9")]
    public void GivenNonAsciiCharacter_WhenSingleClassRegex_ThenNotMatch(string value)
    {
        Assert.DoesNotMatch(RegexCollection.HasNumber(), value);
        Assert.DoesNotMatch(RegexCollection.HasLowerChar(), value);
        Assert.DoesNotMatch(RegexCollection.HasUpperChar(), value);
    }
}
