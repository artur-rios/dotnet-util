using ArturRios.Util.Collections;
using ArturRios.Util.Text;

namespace ArturRios.Util.Tests.Collections;

public class CharactersTests
{
    [Fact]
    public void GivenSpecial_WhenInspected_ThenCoverEveryNonAlphanumericPrintableAsciiCharacter()
    {
        var expected = Enumerable.Range(33, 126 - 33 + 1)
            .Select(code => (char)code)
            .Where(character => !char.IsAsciiLetterOrDigit(character))
            .Order()
            .ToArray();

        Assert.Equal(expected, Characters.Special.Order());
    }

    [Theory]
    [InlineData('`')]
    [InlineData('~')]
    [InlineData('\\')]
    public void GivenCharacterTheEmailPatternAccepts_WhenHasSpecialChar_ThenTreatItAsSpecial(char character)
    {
        // These three were missing from the pool, so HasSpecialChar disagreed with the email pattern.
        Assert.Contains(character, Characters.Special);
        Assert.True(character.ToString().HasSpecialChar());
    }

    [Fact]
    public void GivenSpecial_WhenInspected_ThenContainNoDuplicatesAndNoWhitespace()
    {
        Assert.Equal(Characters.Special.Length, Characters.Special.Distinct().Count());
        Assert.DoesNotContain(Characters.Special, char.IsWhiteSpace);
    }

    [Fact]
    public void GivenEveryPool_WhenCombined_ThenAllIsTheirConcatenationWithoutOverlap()
    {
        var expectedLength = Characters.Digits.Length + Characters.LowerLetters.Length +
                             Characters.UpperLetters.Length + Characters.Special.Length;

        Assert.Equal(expectedLength, Characters.All.Length);
        Assert.Equal(expectedLength, Characters.All.Distinct().Count());
    }

    [Fact]
    public void GivenEveryCharacterInAll_WhenClassified_ThenExactlyOneClassIsReported()
    {
        foreach (var character in Characters.All)
        {
            var classes = character.ToString().Classify();

            Assert.NotEqual(CharacterClasses.None, classes);
            Assert.Equal(1, System.Numerics.BitOperations.PopCount((uint)classes));
        }
    }

    [Fact]
    public void GivenDigitsLowerAndUpper_WhenInspected_ThenMatchTheAsciiRanges()
    {
        Assert.Equal("0123456789", Characters.Digits);
        Assert.Equal("abcdefghijklmnopqrstuvwxyz", Characters.LowerLetters);
        Assert.Equal("ABCDEFGHIJKLMNOPQRSTUVWXYZ", Characters.UpperLetters);
    }
}
