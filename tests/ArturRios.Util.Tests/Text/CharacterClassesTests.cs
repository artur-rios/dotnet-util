using ArturRios.Util.RegularExpressions;
using ArturRios.Util.Text;

namespace ArturRios.Util.Tests.Text;

[Trait("Category", "Unit")]
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

        Assert.DoesNotMatch(RegexCollection.HasNumberLowerAndUpperChar(), value);
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

    [Theory]
    [InlineData("")]
    [InlineData("aB1")]
    [InlineData("aB1!")]
    [InlineData("\u0663 \u00df")]
    public void GivenSpan_WhenClassifyOrMissing_ThenSameResultAsTheStringOverload(string value)
    {
        const CharacterClasses required = CharacterClasses.Digit | CharacterClasses.Special;

        Assert.Equal(value.Classify(), value.AsSpan().Classify());
        Assert.Equal(value.Missing(required), value.AsSpan().Missing(required));
    }

    [Fact]
    public void GivenNullString_WhenMissing_ThenEveryRequirementIsMissing()
    {
        string? value = null;

        const CharacterClasses required = CharacterClasses.Lower | CharacterClasses.Upper;

        Assert.Equal(required, value.Missing(required));
    }

    [Fact]
    public void GivenNoRequirement_WhenMissing_ThenNothingIsMissing()
    {
        Assert.Equal(CharacterClasses.None, string.Empty.Missing(CharacterClasses.None));
    }

    [Fact]
    public void GivenALongStringWithEveryClassUpFront_WhenClassify_ThenAllFlags()
    {
        // Classify stops reading once every class has been seen; the answer must not change because of it.
        var value = "aB1!" + new string(' ', 10_000);

        Assert.Equal(CharacterClasses.Digit | CharacterClasses.Lower | CharacterClasses.Upper | CharacterClasses.Special,
            value.Classify());
    }

    [Fact]
    public void GivenTheFlags_WhenInspected_ThenEachIsADistinctBit()
    {
        Assert.Equal(0, (int)CharacterClasses.None);
        Assert.Equal([1, 2, 4, 8],
            new[] { CharacterClasses.Digit, CharacterClasses.Lower, CharacterClasses.Upper, CharacterClasses.Special }
                .Select(flag => (int)flag));
    }

    [Fact]
    public void GivenNonAsciiDigit_WhenClassify_ThenOnlyTheCompositeRegexCountsItAsDocumented()
    {
        // Pins the one documented divergence: the composite pattern's \d is Unicode-aware, Classify is not.
        const string value = "abcABC\u0663";
        var required = CharacterClasses.Digit | CharacterClasses.Lower | CharacterClasses.Upper;

        Assert.Matches(RegexCollection.HasNumberLowerAndUpperChar(), value);
        Assert.Equal(CharacterClasses.Digit, value.Missing(required));
    }
}
