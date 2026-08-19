using ArturRios.Util.RegularExpressions;
using ArturRios.Util.Text;

namespace ArturRios.Util.Tests.Text;

public class CharacterClassesTests
{
    [Fact]
    public void GivenEmptyString_WhenClassify_ThenNone()
    {
        Assert.Equal(CharacterClasses.None, string.Empty.Classify());
    }

    [Fact]
    public void GivenNullString_WhenClassify_ThenNone()
    {
        string? value = null;

        Assert.Equal(CharacterClasses.None, value.Classify());
    }

    [Fact]
    public void GivenStringWithEveryClass_WhenClassify_ThenAllFlags()
    {
        var expected = CharacterClasses.Digit | CharacterClasses.Lower | CharacterClasses.Upper |
                       CharacterClasses.Special;

        Assert.Equal(expected, "aB1!".Classify());
    }

    [Theory]
    [InlineData("1", CharacterClasses.Digit)]
    [InlineData("a", CharacterClasses.Lower)]
    [InlineData("A", CharacterClasses.Upper)]
    [InlineData("!", CharacterClasses.Special)]
    [InlineData("aB", CharacterClasses.Lower | CharacterClasses.Upper)]
    public void GivenString_WhenClassify_ThenOnlyPresentClassesAreFlagged(string value, CharacterClasses expected)
    {
        Assert.Equal(expected, value.Classify());
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("٣")]
    [InlineData("Ⅷ")]
    public void GivenCharOutsideEveryAsciiClass_WhenClassify_ThenNone(string value)
    {
        Assert.Equal(CharacterClasses.None, value.Classify());
    }

    [Fact]
    public void GivenStringSpanningLines_WhenClassify_ThenClassesAreStillDetected()
    {
        var expected = CharacterClasses.Digit | CharacterClasses.Lower | CharacterClasses.Upper;
        const string value = "aB\n1";

        Assert.False(RegexCollection.HasNumberLowerAndUpperChar().IsMatch(value));
        Assert.Equal(expected, value.Classify());
    }

    [Fact]
    public void GivenStringMeetingEveryRequirement_WhenMissing_ThenNone()
    {
        var required = CharacterClasses.Digit | CharacterClasses.Lower | CharacterClasses.Upper;

        Assert.Equal(CharacterClasses.None, "aB1".Missing(required));
    }

    [Fact]
    public void GivenStringMissingRequirements_WhenMissing_ThenOnlyTheAbsentOnes()
    {
        var required = CharacterClasses.Digit | CharacterClasses.Lower | CharacterClasses.Upper |
                       CharacterClasses.Special;

        Assert.Equal(CharacterClasses.Upper | CharacterClasses.Special, "ab1".Missing(required));
    }

    [Fact]
    public void GivenClassPresentButNotRequired_WhenMissing_ThenItIsNotReported()
    {
        Assert.Equal(CharacterClasses.None, "aB1!".Missing(CharacterClasses.Digit));
    }

    [Theory]
    [InlineData("abcABC123")]
    [InlineData("abc")]
    [InlineData("aB1")]
    [InlineData("ABC123")]
    [InlineData("abc123")]
    public void GivenSingleLineString_WhenClassify_ThenAgreesWithCompositeRegex(string value)
    {
        var required = CharacterClasses.Digit | CharacterClasses.Lower | CharacterClasses.Upper;

        Assert.Equal(RegexCollection.HasNumberLowerAndUpperChar().IsMatch(value), value.Missing(required) == CharacterClasses.None);
    }

    [Fact]
    public void GivenSpan_WhenClassify_ThenSameResultAsString()
    {
        ReadOnlySpan<char> span = "aB1".AsSpan();

        Assert.Equal(CharacterClasses.Digit | CharacterClasses.Lower | CharacterClasses.Upper, span.Classify());
    }
}
