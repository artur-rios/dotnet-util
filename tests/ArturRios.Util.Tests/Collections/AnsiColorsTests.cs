using System.Reflection;
using ArturRios.Util.Collections;

namespace ArturRios.Util.Tests.Collections;

[Trait("Category", "Unit")]
public class AnsiColorsTests
{
    private const char Escape = '\x1b';

    public static TheoryData<string> AllCodes =>
    [
        AnsiColors.Reset,
        AnsiColors.DarkGray,
        AnsiColors.Cyan,
        AnsiColors.White,
        AnsiColors.Yellow,
        AnsiColors.Red,
        AnsiColors.Magenta,
        AnsiColors.BrightRed,
        AnsiColors.Green
    ];

    [Theory]
    [MemberData(nameof(AllCodes))]
    public void GivenAnyDeclaredCode_WhenInspected_ThenItIsAWellFormedAnsiSelectGraphicRenditionSequence(string code)
    {
        Assert.StartsWith($"{Escape}[", code);
        Assert.EndsWith("m", code);
        Assert.All(code[2..^1], character => Assert.True(char.IsAsciiDigit(character) || character == ';'));
    }

    [Fact]
    public void GivenTheDeclaredCodes_WhenCompared_ThenNoTwoColoursShareASequence()
    {
        var codes = typeof(AnsiColors)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field is { IsLiteral: true, IsInitOnly: false })
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToArray();

        Assert.Equal(codes.Length, codes.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void GivenTheResetCode_WhenInspected_ThenItIsTheStandardSelectGraphicRenditionReset()
    {
        Assert.Equal(AnsiColors.Reset, $"{Escape}[0m");
    }

    [Fact]
    public void GivenAColouredString_WhenTerminatedWithReset_ThenTheSequenceWrapsTheTextExactly()
    {
        var coloured = $"{AnsiColors.Green}Success!{AnsiColors.Reset}";

        Assert.Equal($"{Escape}[32mSuccess!{Escape}[0m", coloured);
    }

    [Fact]
    public void GivenTheColourCodes_WhenReset_ThenResetIsNotItselfAColour()
    {
        var colours = new[]
        {
            AnsiColors.DarkGray, AnsiColors.Cyan, AnsiColors.White, AnsiColors.Yellow,
            AnsiColors.Red, AnsiColors.Magenta, AnsiColors.BrightRed, AnsiColors.Green
        };

        Assert.DoesNotContain(AnsiColors.Reset, colours);
    }
}
