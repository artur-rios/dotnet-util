using ArturRios.Util.RegularExpressions;

namespace ArturRios.Util.Tests.Collections;

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
        "中文@hostname.com"
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
        "ma@[10.0.0.255]"
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
    public void GivenStringWithoutNumberLowerAndUpperChar_WhenCompositeRegex_ThenNotMatch(string @string)
    {
        var result = RegexCollection.HasNumberLowerAndUpperChar().IsMatch(@string);

        Assert.False(result);
    }
}
