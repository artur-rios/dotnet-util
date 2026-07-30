namespace ArturRios.Util.Tests.Random;

using ArturRios.Util.Random;
using ArturRios.Util.Collections;

public class CustomRandomTests
{
    [Fact]
    public void GivenRngRange_WhenNumberFromRng_ThenReturnValueWithinRange()
    {
        const int start = 5;
        const int end = 10;
        
        var value = CustomRandom.NumberFromRng(start, end);
        
        Assert.InRange(value, start, end);
    }

    [Fact]
    public void GivenRngRangeWithExcluded_WhenNumberFromRng_ThenReturnDifferentValueFromExcluded()
    {
        const int start = 1;
        const int end = 3;
        const int excluded = 2;
        
        var value = CustomRandom.NumberFromRng(start, end, excluded);
        
        Assert.InRange(value, start, end);
        Assert.NotEqual(excluded, value);
    }

    [Fact]
    public void GivenSystemRandomRange_WhenNumberFromSystemRandom_ThenReturnValueWithinRange()
    {
        const int start = 0;
        const int end = 100;
        
        var value = CustomRandom.NumberFromSystemRandom(start, end);
        
        Assert.InRange(value, start, end - 1);
    }

    [Fact]
    public void GivenSystemRandomRangeWithExcluded_WhenNumberFromSystemRandom_ThenReturnDifferentValueFromExcluded()
    {
        const int start = 10;
        const int end = 20;
        const int excluded = 15;
        
        var value = CustomRandom.NumberFromSystemRandom(start, end, excluded);
        
        Assert.InRange(value, start, end - 1);
        Assert.NotEqual(excluded, value);
    }

    [Fact]
    public void GivenOptions_WhenText_ThenReturnStringWithRequestedLength()
    {
        var options = new RandomStringOptions { Length = 25 };
        
        var result = CustomRandom.Text(options);
        
        Assert.Equal(options.Length, result.Length);
    }

    [Fact]
    public void GivenAllCharacterSetsIncluded_WhenText_ThenContainAtLeastOneFromEachSet()
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
    public void GivenExcludedStrings_WhenText_ThenNotReturnAnyExcludedStrings()
    {
        var options = new RandomStringOptions { Length = 16 };

        var excluded = new[] { "AAAAAAAAAAAAAAAA", "BBBBBBBBBBBBBBBB", "CCCCCCCCCCCCCCCC" };

        var result = CustomRandom.Text(options, excluded);

        Assert.DoesNotContain(result, excluded);
    }

    /// <summary>
    /// Number of generations each randomised assertion runs, so a defect that only
    /// shows up for some draws cannot pass by luck.
    /// </summary>
    private const int Repetitions = 25;

    [Theory]
    [InlineData(true, false, false, false, Characters.LowerLetters)]
    [InlineData(false, true, false, false, Characters.UpperLetters)]
    [InlineData(false, false, true, false, Characters.Digits)]
    [InlineData(false, false, false, true, Characters.Special)]
    [InlineData(true, true, false, false, Characters.LowerLetters + Characters.UpperLetters)]
    [InlineData(true, false, true, false, Characters.LowerLetters + Characters.Digits)]
    [InlineData(true, false, false, true, Characters.LowerLetters + Characters.Special)]
    [InlineData(false, true, true, false, Characters.UpperLetters + Characters.Digits)]
    [InlineData(false, true, false, true, Characters.UpperLetters + Characters.Special)]
    [InlineData(false, false, true, true, Characters.Digits + Characters.Special)]
    [InlineData(true, true, true, false, Characters.LowerLetters + Characters.UpperLetters + Characters.Digits)]
    [InlineData(true, true, false, true, Characters.LowerLetters + Characters.UpperLetters + Characters.Special)]
    [InlineData(true, false, true, true, Characters.LowerLetters + Characters.Digits + Characters.Special)]
    [InlineData(false, true, true, true, Characters.UpperLetters + Characters.Digits + Characters.Special)]
    [InlineData(true, true, true, true, Characters.LowerLetters + Characters.UpperLetters + Characters.Digits + Characters.Special)]
    public void GivenCharacterSetSelection_WhenText_ThenReturnOnlyCharactersFromSelectedSets(
        bool lowercase, bool uppercase, bool digits, bool special, string allowed)
    {
        var options = new RandomStringOptions
        {
            Length = 40,
            IncludeLowercase = lowercase,
            IncludeUppercase = uppercase,
            IncludeDigits = digits,
            IncludeSpecialCharacters = special
        };

        for (var i = 0; i < Repetitions; i++)
        {
            var result = CustomRandom.Text(options);

            Assert.All(result, c => Assert.True(allowed.Contains(c), $"'{c}' is not in the requested character sets: {result}"));
        }
    }

    [Fact]
    public void GivenSpecialCharactersExcluded_WhenText_ThenReturnNoSpecialCharacters()
    {
        var options = new RandomStringOptions
        {
            Length = 48,
            IncludeLowercase = true,
            IncludeUppercase = true,
            IncludeDigits = true,
            IncludeSpecialCharacters = false
        };

        for (var i = 0; i < Repetitions; i++)
        {
            var result = CustomRandom.Text(options);

            Assert.All(result, c => Assert.False(Characters.Special.Contains(c), $"'{c}' is a special character: {result}"));
        }
    }

    [Fact]
    public void GivenOnlyLowercaseIncluded_WhenText_ThenContainAtLeastOneLowercaseCharacter()
    {
        var options = new RandomStringOptions
        {
            Length = 12,
            IncludeLowercase = true,
            IncludeUppercase = false,
            IncludeDigits = false,
            IncludeSpecialCharacters = false
        };

        var result = CustomRandom.Text(options);

        Assert.Contains(result, c => Characters.LowerLetters.Contains(c));
    }

    [Fact]
    public void GivenOnlyUppercaseIncluded_WhenText_ThenContainAtLeastOneUppercaseCharacter()
    {
        var options = new RandomStringOptions
        {
            Length = 12,
            IncludeLowercase = false,
            IncludeUppercase = true,
            IncludeDigits = false,
            IncludeSpecialCharacters = false
        };

        var result = CustomRandom.Text(options);

        Assert.Contains(result, c => Characters.UpperLetters.Contains(c));
    }

    [Fact]
    public void GivenOnlyDigitsIncluded_WhenText_ThenContainAtLeastOneDigit()
    {
        var options = new RandomStringOptions
        {
            Length = 12,
            IncludeLowercase = false,
            IncludeUppercase = false,
            IncludeDigits = true,
            IncludeSpecialCharacters = false
        };

        var result = CustomRandom.Text(options);

        Assert.Contains(result, c => Characters.Digits.Contains(c));
    }

    [Fact]
    public void GivenOnlySpecialCharactersIncluded_WhenText_ThenContainAtLeastOneSpecialCharacter()
    {
        var options = new RandomStringOptions
        {
            Length = 12,
            IncludeLowercase = false,
            IncludeUppercase = false,
            IncludeDigits = false,
            IncludeSpecialCharacters = true
        };

        var result = CustomRandom.Text(options);

        Assert.Contains(result, c => Characters.Special.Contains(c));
    }

    [Fact]
    public void GivenLengthEqualToEnabledSetCount_WhenText_ThenContainExactlyOneCharacterFromEachEnabledSet()
    {
        var options = new RandomStringOptions
        {
            Length = 4,
            IncludeLowercase = true,
            IncludeUppercase = true,
            IncludeDigits = true,
            IncludeSpecialCharacters = true
        };

        for (var i = 0; i < Repetitions; i++)
        {
            var result = CustomRandom.Text(options);

            Assert.Equal(1, result.Count(c => Characters.LowerLetters.Contains(c)));
            Assert.Equal(1, result.Count(c => Characters.UpperLetters.Contains(c)));
            Assert.Equal(1, result.Count(c => Characters.Digits.Contains(c)));
            Assert.Equal(1, result.Count(c => Characters.Special.Contains(c)));
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(48)]
    [InlineData(256)]
    public void GivenSingleCharacterSetAndLength_WhenText_ThenReturnStringWithRequestedLength(int length)
    {
        var options = new RandomStringOptions
        {
            Length = length,
            IncludeLowercase = true,
            IncludeUppercase = false,
            IncludeDigits = false,
            IncludeSpecialCharacters = false
        };

        var result = CustomRandom.Text(options);

        Assert.Equal(length, result.Length);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(48)]
    [InlineData(256)]
    public void GivenAllCharacterSetsAndLength_WhenText_ThenReturnStringWithRequestedLength(int length)
    {
        var options = new RandomStringOptions
        {
            Length = length,
            IncludeLowercase = true,
            IncludeUppercase = true,
            IncludeDigits = true,
            IncludeSpecialCharacters = true
        };

        var result = CustomRandom.Text(options);

        Assert.Equal(length, result.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void GivenLengthSmallerThanEnabledSetCount_WhenText_ThenThrowArgumentException(int length)
    {
        var options = new RandomStringOptions
        {
            Length = length,
            IncludeLowercase = true,
            IncludeUppercase = true,
            IncludeDigits = true,
            IncludeSpecialCharacters = true
        };

        Assert.Throws<ArgumentException>(() => CustomRandom.Text(options));
    }

    [Fact]
    public void GivenNonPositiveLength_WhenText_ThenThrowArgumentException()
    {
        var options = new RandomStringOptions
        {
            Length = 0,
            IncludeLowercase = true,
            IncludeUppercase = false,
            IncludeDigits = false,
            IncludeSpecialCharacters = false
        };

        Assert.Throws<ArgumentException>(() => CustomRandom.Text(options));
    }

    [Fact]
    public void GivenNoCharacterSetsIncluded_WhenText_ThenThrowArgumentException()
    {
        var options = new RandomStringOptions
        {
            Length = 10,
            IncludeLowercase = false,
            IncludeUppercase = false,
            IncludeDigits = false,
            IncludeSpecialCharacters = false
        };

        Assert.Throws<ArgumentException>(() => CustomRandom.Text(options));
    }

    [Fact]
    public void GivenNullOptions_WhenText_ThenThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => CustomRandom.Text(null!));
    }

    [Fact]
    public void GivenAllCharacterSetsIncluded_WhenTextCalledRepeatedly_ThenGuaranteedCharactersAreNotAlwaysInTheSamePosition()
    {
        var options = new RandomStringOptions
        {
            Length = 16,
            IncludeLowercase = true,
            IncludeUppercase = true,
            IncludeDigits = true,
            IncludeSpecialCharacters = true
        };

        var firstCharacters = new HashSet<char>();

        for (var i = 0; i < 100; i++)
        {
            firstCharacters.Add(CustomRandom.Text(options)[0]);
        }

        Assert.Contains(firstCharacters, c => Characters.Special.Contains(c));
    }

    [Fact]
    public void GivenSameOptions_WhenTextCalledRepeatedly_ThenReturnDistinctValues()
    {
        var options = new RandomStringOptions { Length = 32 };

        var results = new HashSet<string>();

        for (var i = 0; i < 100; i++)
        {
            results.Add(CustomRandom.Text(options));
        }

        Assert.Equal(100, results.Count);
    }
}
