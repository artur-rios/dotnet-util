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

    // Unsupported element types such as decimal or double no longer compile, because the generator is
    // constrained to IBinaryInteger<T>. The former run-time ArgumentException has become a compile error.

    [Fact]
    public void GivenUshortGenerator_WhenNextCalledRepeatedly_ThenReturnPrimesInOrder()
    {
        var generator = new PrimeGenerator<ushort>();

        Assert.Equal((ushort)2, generator.Next());
        Assert.Equal((ushort)3, generator.Next());
        Assert.Equal((ushort)5, generator.Next());
        Assert.Equal((ushort)7, generator.Next());
    }

    [Fact]
    public void GivenUintGenerator_WhenNextCalledRepeatedly_ThenReturnPrimesInOrder()
    {
        var generator = new PrimeGenerator<uint>();

        Assert.Equal(2U, generator.Next());
        Assert.Equal(3U, generator.Next());
        Assert.Equal(5U, generator.Next());
        Assert.Equal(7U, generator.Next());
    }

    [Fact]
    public void GivenUlongGenerator_WhenNextCalledRepeatedly_ThenReturnPrimesInOrder()
    {
        var generator = new PrimeGenerator<ulong>();

        Assert.Equal(2UL, generator.Next());
        Assert.Equal(3UL, generator.Next());
        Assert.Equal(5UL, generator.Next());
        Assert.Equal(7UL, generator.Next());
    }

    [Fact]
    public void GivenShortGenerator_WhenNextCalledRepeatedly_ThenReturnPrimesInOrder()
    {
        var generator = new PrimeGenerator<short>();

        Assert.Equal((short)2, generator.Next());
        Assert.Equal((short)3, generator.Next());
        Assert.Equal((short)5, generator.Next());
        Assert.Equal((short)7, generator.Next());
    }

    [Fact]
    public void GivenByteGenerator_WhenExceedingLargestPrimeInRange_ThenThrowOverflowException()
    {
        var generator = new PrimeGenerator<byte>();

        byte last = 0;

        Assert.Throws<OverflowException>((Action)ExhaustPrimes);

        // 251 is the largest prime that fits in a byte.
        Assert.Equal((byte)251, last);

        return;

        void ExhaustPrimes()
        {
            while (true)
            {
                last = generator.Next();
            }
        }
    }

    [Fact]
    public void GivenExhaustedGenerator_WhenNextCalledAgain_ThenThrowOverflowExceptionWithoutWrapping()
    {
        var generator = new PrimeGenerator<sbyte>();

        while (true)
        {
            try
            {
                generator.Next();
            }
            catch (OverflowException)
            {
                break;
            }
        }

        // The generator stays parked on its last prime instead of restarting the sequence.
        Assert.Throws<OverflowException>(() => generator.Next());
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
