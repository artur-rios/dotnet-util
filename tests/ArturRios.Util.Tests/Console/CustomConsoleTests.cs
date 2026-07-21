using ArturRios.Util.Console;

namespace ArturRios.Util.Tests.Console;

public class CustomConsoleTests
{
    private static string CaptureOutput(Action action)
    {
        var original = System.Console.Out;
        using var writer = new StringWriter();

        System.Console.SetOut(writer);

        try
        {
            action();
        }
        finally
        {
            System.Console.SetOut(original);
        }

        return writer.ToString();
    }

    [Fact]
    public void GivenNoArguments_WhenWriteCharLine_ThenWriteHundredDashesFollowedByNewLine()
    {
        var output = CaptureOutput(() => CustomConsole.WriteCharLine());

        Assert.Equal(new string('-', 100) + Environment.NewLine, output);
    }

    [Fact]
    public void GivenOnlyCharArgument_WhenWriteCharLine_ThenWriteHundredOfThatChar()
    {
        var output = CaptureOutput(() => CustomConsole.WriteCharLine('*'));

        Assert.Equal(new string('*', 100) + Environment.NewLine, output);
    }

    [Theory]
    [InlineData('=', 5)]
    [InlineData('#', 1)]
    [InlineData('-', 3)]
    [InlineData(' ', 4)]
    [InlineData('\t', 2)]
    [InlineData('á', 6)]
    [InlineData('─', 8)]
    public void GivenCharAndQuantity_WhenWriteCharLine_ThenWriteThatCharRepeatedQuantityTimes(char character,
        int quantity)
    {
        var output = CaptureOutput(() => CustomConsole.WriteCharLine(character, quantity));

        Assert.Equal(new string(character, quantity) + Environment.NewLine, output);
    }

    [Fact]
    public void GivenZeroQuantity_WhenWriteCharLine_ThenWriteOnlyNewLine()
    {
        var output = CaptureOutput(() => CustomConsole.WriteCharLine('#', 0));

        Assert.Equal(Environment.NewLine, output);
    }

    [Fact]
    public void GivenLargeQuantity_WhenWriteCharLine_ThenWriteAllRequestedChars()
    {
        var output = CaptureOutput(() => CustomConsole.WriteCharLine('.', 10_000));

        Assert.Equal(new string('.', 10_000) + Environment.NewLine, output);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void GivenNegativeQuantity_WhenWriteCharLine_ThenThrowArgumentOutOfRangeException(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CustomConsole.WriteCharLine('-', quantity));
    }

    [Fact]
    public void GivenMultipleCalls_WhenWriteCharLine_ThenWriteOneLinePerCall()
    {
        var output = CaptureOutput(() =>
        {
            CustomConsole.WriteCharLine('a', 2);
            CustomConsole.WriteCharLine('b', 3);
        });

        Assert.Equal($"aa{Environment.NewLine}bbb{Environment.NewLine}", output);
    }
}
