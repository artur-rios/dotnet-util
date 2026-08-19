using ArturRios.Util.RegularExpressions;
using ArturRios.Util.Text;

namespace ArturRios.Util.Tests.Text;

public class EmailAddressTests
{
    [Fact]
    public void GivenMixedCaseDomain_WhenTryNormalize_ThenDomainIsLowercased()
    {
        var normalized = EmailAddress.TryNormalize("MA@Hostname.COM", out var result);

        Assert.True(normalized);
        Assert.Equal("MA@hostname.com", result);
    }

    [Fact]
    public void GivenInternationalizedDomain_WhenTryNormalize_ThenDomainIsPunycoded()
    {
        var normalized = EmailAddress.TryNormalize("ma@münchen.de", out var result);

        Assert.True(normalized);
        Assert.Equal("ma@xn--mnchen-3ya.de", result);
    }

    [Fact]
    public void GivenIpLiteralDomain_WhenTryNormalize_ThenLeftUntouched()
    {
        var normalized = EmailAddress.TryNormalize("ma@[192.168.1.1]", out var result);

        Assert.True(normalized);
        Assert.Equal("ma@[192.168.1.1]", result);
    }

    [Fact]
    public void GivenAlreadyNormalizedAddress_WhenTryNormalize_ThenUnchanged()
    {
        var normalized = EmailAddress.TryNormalize("ma@hostname.com", out var result);

        Assert.True(normalized);
        Assert.Equal("ma@hostname.com", result);
    }

    [Fact]
    public void GivenTwoSpellingsOfTheSameAddress_WhenTryNormalize_ThenBothProduceTheSameResult()
    {
        EmailAddress.TryNormalize("MA@HOSTNAME.COM", out var upper);
        EmailAddress.TryNormalize("MA@hostname.com", out var lower);

        Assert.Equal(lower, upper);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("hostname.com")]
    [InlineData("@hostname.com")]
    [InlineData("ma@")]
    [InlineData("ma@-hostname.com")]
    [InlineData("ma@hostname-.com")]
    [InlineData("ma@1.2.3.999")]
    [InlineData("ma@192.168.1.1")]
    [InlineData("ma@hostname.com\n")]
    [InlineData(" ma@hostname.com ")]
    [InlineData("John Doe <ma@hostname.com>")]
    [InlineData("ma@hostname.com, other@hostname.com")]
    public void GivenInvalidAddress_WhenTryNormalize_ThenFalseAndNullResult(string? value)
    {
        var normalized = EmailAddress.TryNormalize(value, out var result);

        Assert.False(normalized);
        Assert.Null(result);
    }

    [Fact]
    public void GivenNonAsciiLocalPart_WhenTryNormalize_ThenRejected()
    {
        var normalized = EmailAddress.TryNormalize("joão@hostname.com", out var result);

        Assert.False(normalized);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("ma@hostname.com")]
    [InlineData("ma-a.aa@hostname.com.edu")]
    [InlineData("ma@hostname.museum")]
    [InlineData("ma@[10.0.0.255]")]
    public void GivenAddressTheRegexAccepts_WhenIsValid_ThenTrue(string value)
    {
        Assert.True(RegexCollection.Email().IsMatch(value));
        Assert.True(EmailAddress.IsValid(value));
    }

    [Fact]
    public void GivenInternationalizedDomain_WhenIsValid_ThenTrueEvenThoughTheRegexRejectsIt()
    {
        const string value = "ma@münchen.de";

        Assert.False(RegexCollection.Email().IsMatch(value));
        Assert.True(EmailAddress.IsValid(value));
    }
}
