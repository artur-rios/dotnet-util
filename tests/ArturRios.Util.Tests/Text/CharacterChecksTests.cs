using ArturRios.Util.RegularExpressions;
using ArturRios.Util.Text;

namespace ArturRios.Util.Tests.Text;

public class CharacterChecksTests
{
    [Theory]
    [InlineData("ABC123", true)]
    [InlineData("ABC", false)]
    [InlineData("", false)]
    [InlineData("0", true)]
    [InlineData("9", true)]
    public void GivenString_WhenHasNumber_ThenReportsWhetherAnAsciiDigitIsPresent(string value, bool expected)
    {
        Assert.Equal(expected, value.HasNumber());
    }

    [Theory]
    [InlineData("ABCabc", true)]
    [InlineData("ABC", false)]
    [InlineData("", false)]
    public void GivenString_WhenHasLowerChar_ThenReportsWhetherAnAsciiLowercaseIsPresent(string value, bool expected)
    {
        Assert.Equal(expected, value.HasLowerChar());
    }

    [Theory]
    [InlineData("abcABC", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    public void GivenString_WhenHasUpperChar_ThenReportsWhetherAnAsciiUppercaseIsPresent(string value, bool expected)
    {
        Assert.Equal(expected, value.HasUpperChar());
    }

    [Theory]
    [InlineData("abc!", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    public void GivenString_WhenHasSpecialChar_ThenReportsWhetherASpecialCharIsPresent(string value, bool expected)
    {
        Assert.Equal(expected, value.HasSpecialChar());
    }

    [Fact]
    public void GivenNullString_WhenHasNumber_ThenFalse()
    {
        string? value = null;

        Assert.False(value.HasNumber());
    }

    [Fact]
    public void GivenNullString_WhenHasLowerChar_ThenFalse()
    {
        string? value = null;

        Assert.False(value.HasLowerChar());
    }

    [Fact]
    public void GivenNullString_WhenHasUpperChar_ThenFalse()
    {
        string? value = null;

        Assert.False(value.HasUpperChar());
    }

    [Fact]
    public void GivenNullString_WhenHasSpecialChar_ThenFalse()
    {
        string? value = null;

        Assert.False(value.HasSpecialChar());
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("aB1")]
    [InlineData("٣")]
    [InlineData("ß")]
    [InlineData("Ⅷ")]
    public void GivenString_WhenHasNumber_ThenAgreesWithHasNumberRegex(string value)
    {
        Assert.Equal(RegexCollection.HasNumber().IsMatch(value), value.HasNumber());
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("aB1")]
    [InlineData("٣")]
    [InlineData("ß")]
    [InlineData("Ⅷ")]
    public void GivenString_WhenHasLowerChar_ThenAgreesWithHasLowerCharRegex(string value)
    {
        Assert.Equal(RegexCollection.HasLowerChar().IsMatch(value), value.HasLowerChar());
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("aB1")]
    [InlineData("٣")]
    [InlineData("ß")]
    [InlineData("Ⅷ")]
    public void GivenString_WhenHasUpperChar_ThenAgreesWithHasUpperCharRegex(string value)
    {
        Assert.Equal(RegexCollection.HasUpperChar().IsMatch(value), value.HasUpperChar());
    }

    [Fact]
    public void GivenNonAsciiDigit_WhenHasNumber_ThenFalseUnlikeCharIsDigit()
    {
        const string arabicIndicThree = "٣";

        Assert.Contains(arabicIndicThree, char.IsDigit);
        Assert.False(arabicIndicThree.HasNumber());
    }

    [Fact]
    public void GivenNonAsciiLowercase_WhenHasLowerChar_ThenFalseUnlikeCharIsLower()
    {
        const string sharpS = "ß";

        Assert.Contains(sharpS, char.IsLower);
        Assert.False(sharpS.HasLowerChar());
    }

    [Fact]
    public void GivenSpan_WhenHasNumber_ThenSameResultAsString()
    {
        ReadOnlySpan<char> span = "abc1".AsSpan();

        Assert.True(span.HasNumber());
    }
}
