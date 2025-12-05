namespace ArturRios.Util.Tests.Random;

using ArturRios.Util.Random;
using ArturRios.Util.Collections;

public class CustomRandomTests
{
    [Fact]
    public void Should_ReturnValueWithinRange()
    {
        const int start = 5;
        const int end = 10;
        
        var value = CustomRandom.NumberFromRng(start, end);
        
        Assert.InRange(value, start, end);
    }

    [Fact]
    public void Should_ReturnDifferentValueFromExcluded()
    {
        const int start = 1;
        const int end = 3;
        const int excluded = 2;
        
        var value = CustomRandom.NumberFromRng(start, end, excluded);
        
        Assert.InRange(value, start, end);
        Assert.NotEqual(excluded, value);
    }

    [Fact]
    public void Should_ReturnValueWithinRangeOnNumberFromSystemRandom()
    {
        const int start = 0;
        const int end = 100;
        
        var value = CustomRandom.NumberFromSystemRandom(start, end);
        
        Assert.InRange(value, start, end - 1);
    }

    [Fact]
    public void Should_ReturnDifferentValueFromExcludedOnNumberFromSystemRandom()
    {
        const int start = 10;
        const int end = 20;
        const int excluded = 15;
        
        var value = CustomRandom.NumberFromSystemRandom(start, end, excluded);
        
        Assert.InRange(value, start, end - 1);
        Assert.NotEqual(excluded, value);
    }

    [Fact]
    public void Should_ReturnStringWithRequestedLength()
    {
        var options = new RandomStringOptions { Length = 25 };
        
        var result = CustomRandom.Text(options);
        
        Assert.Equal(options.Length, result.Length);
    }

    [Fact]
    public void Should_ContainAtLeastOneCharacterFromEachIncludedSet()
    {
        var options = new RandomStringOptions
        {
            Length = 40,
            IncludeLowercase = true,
            IncludeUppercase = true,
            IncludeDigits = true,
            IncludeSpecialCharacters = true
        };
        
        var result = CustomRandom.Text(options);
        
        Assert.Contains(result, c => Characters.LowerLetters.Contains(c));
        Assert.Contains(result, c => Characters.UpperLetters.Contains(c));
        Assert.Contains(result, c => Characters.Digits.Contains(c));
        Assert.Contains(result, c => Characters.Special.Contains(c));
    }

    [Fact]
    public void ShouldNot_ReturnAnyExcludedStrings()
    {
        var options = new RandomStringOptions { Length = 16 };

        var excluded = new[] { "AAAAAAAAAAAAAAAA", "BBBBBBBBBBBBBBBB", "CCCCCCCCCCCCCCCC" };
        
        var result = CustomRandom.Text(options, excluded);
        
        Assert.DoesNotContain(result, excluded);
    }
}