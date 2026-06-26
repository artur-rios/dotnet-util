using System.Numerics;
using ArturRios.Util.Math;

namespace ArturRios.Util.Tests.Math;

public class PrimeGeneratorTests
{
    private static readonly int[] s_firstTenPrimes = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29];

    [Fact]
    public void GivenNewGenerator_WhenNext_ThenReturnFirstPrimeTwo()
    {
        var generator = new PrimeGenerator<int>();

        Assert.Equal(2, generator.Next());
    }

    [Fact]
    public void GivenIntGenerator_WhenNextCalledRepeatedly_ThenReturnPrimesInOrder()
    {
        var generator = new PrimeGenerator<int>();

        foreach (var expected in s_firstTenPrimes)
        {
            Assert.Equal(expected, generator.Next());
        }
    }

    [Fact]
    public void GivenLongGenerator_WhenNextCalledRepeatedly_ThenReturnPrimesInOrder()
    {
        var generator = new PrimeGenerator<long>();

        foreach (var expected in s_firstTenPrimes)
        {
            Assert.Equal(expected, generator.Next());
        }
    }

    [Fact]
    public void GivenByteGenerator_WhenNextCalledRepeatedly_ThenReturnPrimesInOrder()
    {
        var generator = new PrimeGenerator<byte>();

        foreach (var expected in s_firstTenPrimes)
        {
            Assert.Equal((byte)expected, generator.Next());
        }
    }

    [Fact]
    public void GivenBigIntegerGenerator_WhenNextCalledRepeatedly_ThenReturnPrimesInOrder()
    {
        var generator = new PrimeGenerator<BigInteger>();

        foreach (var expected in s_firstTenPrimes)
        {
            Assert.Equal(new BigInteger(expected), generator.Next());
        }
    }

    [Fact]
    public void GivenTwoGenerators_WhenNextCalledIndependently_ThenEachKeepsItsOwnSequence()
    {
        var first = new PrimeGenerator<int>();
        var second = new PrimeGenerator<int>();

        Assert.Equal(2, first.Next());
        Assert.Equal(3, first.Next());

        // The second generator is unaffected by the first and starts from the beginning.
        Assert.Equal(2, second.Next());
        Assert.Equal(3, second.Next());
        Assert.Equal(5, first.Next());
    }

    [Fact]
    public void GivenUnsupportedType_WhenConstructed_ThenThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PrimeGenerator<decimal>());
    }

    [Fact]
    public void GivenSbyteGenerator_WhenExceedingLargestPrimeInRange_ThenThrowOverflowException()
    {
        var generator = new PrimeGenerator<sbyte>();

        sbyte last = 0;

        Assert.Throws<OverflowException>((Action)ExhaustPrimes);

        Assert.Equal(127, last);

        return;

        // 127 is the largest prime that fits in a sbyte; the next call overflows.
        void ExhaustPrimes()
        {
            while (true)
            {
                last = generator.Next();
            }
        }
    }
}
