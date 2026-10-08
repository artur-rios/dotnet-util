using ArturRios.Util.Hashing;

namespace ArturRios.Util.Tests.Hashing;

[Trait("Category", "Unit")]
public class HashConfigurationTests
{
    [Fact]
    public void GivenNoArguments_WhenConstructed_ThenUseTheDocumentedDefaults()
    {
        var configuration = new HashConfiguration();

        Assert.Equal(HashConfiguration.DefaultDegreeOfParallelism, configuration.DegreeOfParallelism);
        Assert.Equal(HashConfiguration.DefaultNumberOfIterations, configuration.NumberOfIterations);
        Assert.Equal(HashConfiguration.DefaultMemoryToUseInKb, configuration.MemoryToUseInKb);
    }

    [Fact]
    public void GivenExplicitValues_WhenConstructed_ThenKeepThem()
    {
        var configuration = new HashConfiguration(degreeOfParallelism: 2, numberOfIterations: 3, memoryToUseInKb: 4096);

        Assert.Equal(2, configuration.DegreeOfParallelism);
        Assert.Equal(3, configuration.NumberOfIterations);
        Assert.Equal(4096, configuration.MemoryToUseInKb);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNonPositiveDegreeOfParallelism_WhenConstructed_ThenThrowArgumentOutOfRangeException(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HashConfiguration(degreeOfParallelism: value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNonPositiveNumberOfIterations_WhenConstructed_ThenThrowArgumentOutOfRangeException(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HashConfiguration(numberOfIterations: value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNonPositiveMemory_WhenConstructed_ThenThrowArgumentOutOfRangeException(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HashConfiguration(memoryToUseInKb: value));
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(4, 15)]
    [InlineData(16, 63)]
    public void GivenLessThanFourKilobytesPerLane_WhenConstructed_ThenThrowArgumentOutOfRangeException(int lanes, int memory)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new HashConfiguration(degreeOfParallelism: lanes, memoryToUseInKb: memory));

        Assert.Equal("memoryToUseInKb", exception.ParamName);
    }

    [Fact]
    public void GivenOnlyASmallMemoryAndTheDefaultLanes_WhenConstructed_ThenThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HashConfiguration(memoryToUseInKb: 32));
    }

    [Theory]
    [InlineData(1, 4)]
    [InlineData(4, 16)]
    public void GivenExactlyFourKilobytesPerLane_WhenHashing_ThenTheConfigurationIsUsable(int lanes, int memory)
    {
        var configuration = new HashConfiguration(degreeOfParallelism: lanes, numberOfIterations: 1, memoryToUseInKb: memory);

        var hash = Hash.EncodeWithSalt("secret", new byte[16], configuration);

        Assert.NotEmpty(hash);
    }
}
