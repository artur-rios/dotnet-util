using System.Globalization;
using System.Numerics;
using ArturRios.Util.Math;

namespace ArturRios.Util.Tests.Math;

[Trait("Category", "Unit")]
public class PrimeUtilsTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(11)]
    [InlineData(13)]
    [InlineData(97)]
    [InlineData(7919)]
    [InlineData(104729)]
    [InlineData(46349)] // the first prime whose square exceeds int.MaxValue
    [InlineData(int.MaxValue)] // 2^31-1, a Mersenne prime: the trial division runs to the top of its range
    public void GivenPrimeInt_WhenIsPrimeNumber_ThenReturnTrue(int value)
    {
        Assert.True(PrimeUtils.IsPrimeNumber(value));
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-7)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(9)]
    [InlineData(100)]
    [InlineData(7917)]
    [InlineData(25)] // a square of a prime: the divisor equals the limit
    [InlineData(46327 * 46337)] // two primes just below the square root of int.MaxValue
    [InlineData(int.MaxValue - 1)]
    public void GivenNonPrimeInt_WhenIsPrimeNumber_ThenReturnFalse(int value)
    {
        Assert.False(PrimeUtils.IsPrimeNumber(value));
    }

    [Theory]
    [InlineData((sbyte)2, true)]
    [InlineData((sbyte)7, true)]
    [InlineData((sbyte)127, true)]
    [InlineData((sbyte)1, false)]
    [InlineData((sbyte)9, false)]
    [InlineData((sbyte)-5, false)]
    public void GivenSbyte_WhenIsPrimeNumber_ThenReturnExpected(sbyte value, bool expected)
    {
        Assert.Equal(expected, PrimeUtils.IsPrimeNumber(value));
    }

    [Theory]
    [InlineData((byte)2, true)]
    [InlineData((byte)251, true)]
    [InlineData((byte)1, false)]
    [InlineData((byte)100, false)]
    public void GivenByte_WhenIsPrimeNumber_ThenReturnExpected(byte value, bool expected)
    {
        Assert.Equal(expected, PrimeUtils.IsPrimeNumber(value));
    }

    [Theory]
    [InlineData((short)13, true)]
    [InlineData((short)7919, true)]
    [InlineData((short)1, false)]
    [InlineData((short)-3, false)]
    [InlineData((short)8, false)]
    public void GivenShort_WhenIsPrimeNumber_ThenReturnExpected(short value, bool expected)
    {
        Assert.Equal(expected, PrimeUtils.IsPrimeNumber(value));
    }

    [Theory]
    [InlineData((ushort)13, true)]
    [InlineData((ushort)65521, true)]
    [InlineData((ushort)1, false)]
    [InlineData((ushort)65520, false)]
    public void GivenUshort_WhenIsPrimeNumber_ThenReturnExpected(ushort value, bool expected)
    {
        Assert.Equal(expected, PrimeUtils.IsPrimeNumber(value));
    }

    [Theory]
    [InlineData(7U, true)]
    [InlineData(2147483647U, true)] // largest int, also prime, exercises the int path
    [InlineData(4294967291U, true)] // largest prime below 2^32, exercises the ulong path
    [InlineData(1U, false)]
    [InlineData(4294967295U, false)] // uint.MaxValue, composite
    public void GivenUint_WhenIsPrimeNumber_ThenReturnExpected(uint value, bool expected)
    {
        Assert.Equal(expected, PrimeUtils.IsPrimeNumber(value));
    }

    [Theory]
    [InlineData(2L, true)]
    [InlineData(7919L, true)]
    [InlineData(2147483647L, true)]
    [InlineData(1000000007L, true)]
    [InlineData(9223372036854775783L, true)] // largest prime below long.MaxValue
    [InlineData(1L, false)]
    [InlineData(1000000005L, false)]
    [InlineData(9223372036854775807L, false)] // long.MaxValue, composite
    [InlineData(0L, false)]
    [InlineData(-1L, false)]
    [InlineData(-2L, false)]
    [InlineData(-7L, false)]
    [InlineData(-59L, false)] // reinterpreting the bits as a ulong yields 2^64-59, which IS prime
    [InlineData(-9223372036854775783L, false)]
    [InlineData(-9223372036854775808L, false)] // long.MinValue
    public void GivenLong_WhenIsPrimeNumber_ThenReturnExpected(long value, bool expected)
    {
        Assert.Equal(expected, PrimeUtils.IsPrimeNumber(value));
    }

    [Theory]
    [InlineData(2UL, true)]
    [InlineData(1000000007UL, true)]
    [InlineData(18446744073709551557UL, true)] // largest prime below 2^64
    [InlineData(0UL, false)]
    [InlineData(1UL, false)]
    [InlineData(9UL, false)]
    [InlineData(18446744073709551615UL, false)] // ulong.MaxValue, composite
    public void GivenUlong_WhenIsPrimeNumber_ThenReturnExpected(ulong value, bool expected)
    {
        Assert.Equal(expected, PrimeUtils.IsPrimeNumber(value));
    }

    [Theory]
    [InlineData("2", true)]
    [InlineData("999983", true)]
    [InlineData("1000000007", true)]
    [InlineData("-7", false)]
    [InlineData("0", false)]
    [InlineData("1", false)]
    [InlineData("1000000", false)]
    public void GivenBigInteger_WhenIsPrimeNumber_ThenReturnExpected(string value, bool expected)
    {
        var bigInteger = Parse(value);

        Assert.Equal(expected, PrimeUtils.IsPrimeNumber(bigInteger));
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("1", "1")]
    [InlineData("4", "2")]
    [InlineData("9", "3")]
    [InlineData("16", "4")]
    [InlineData("100", "10")]
    [InlineData("1000000000000000000", "1000000000")] // 10^18 -> 10^9
    public void GivenPerfectSquare_WhenBigIntegerSqrt_ThenReturnExactRoot(string value, string expected)
    {
        var result = PrimeUtils.BigIntegerSqrt(Parse(value));

        Assert.Equal(Parse(expected), result);
    }

    [Theory]
    [InlineData("2", "1")]
    [InlineData("3", "1")]
    [InlineData("15", "3")]
    [InlineData("24", "4")]
    [InlineData("99", "9")]
    public void GivenNonPerfectSquare_WhenBigIntegerSqrt_ThenReturnFlooredRoot(string value, string expected)
    {
        var result = PrimeUtils.BigIntegerSqrt(Parse(value));

        Assert.Equal(Parse(expected), result);
    }

    [Fact]
    public void GivenNegativeValue_WhenBigIntegerSqrt_ThenThrowArithmeticException()
    {
        Assert.Throws<ArithmeticException>(() => PrimeUtils.BigIntegerSqrt(BigInteger.MinusOne));
    }

    [Theory]
    [InlineData("18446744073709551557", true)] // largest prime below 2^64, the last exact answer
    [InlineData("18446744073709551615", false)] // ulong.MaxValue, composite
    [InlineData("18446744073709551629", true)] // smallest prime above 2^64, first probabilistic answer
    [InlineData("18446744073709551616", false)] // 2^64
    [InlineData("170141183460469231731687303715884105727", true)] // 2^127-1, the Mersenne prime M127
    [InlineData("170141183460469231731687303715884105729", false)]
    public void GivenBigIntegerBeyondSixtyFourBits_WhenIsPrimeNumber_ThenReturnExpected(string value, bool expected)
    {
        Assert.Equal(expected, PrimeUtils.IsPrimeNumber(Parse(value)));
    }

    [Fact]
    public async Task GivenLargeBigIntegerPrime_WhenIsPrimeNumber_ThenCompleteQuicklyInsteadOfTrialDividing()
    {
        // 2^521-1 is the Mersenne prime M521. Trial division would need ~2^260 iterations; Miller-Rabin
        // settles it in milliseconds. Completing at all is the assertion.
        var mersenne521 = BigInteger.Pow(2, 521) - 1;

        var work = Task.Run(() => PrimeUtils.IsPrimeNumber(mersenne521));

        var finished = await Task.WhenAny(work, Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));

        Assert.Same(work, finished);
        Assert.True(await work);
    }

    [Fact]
    public void GivenLargeBigIntegerComposite_WhenIsPrimeNumber_ThenReturnFalse()
    {
        // The square of 2^100+277, a 101-bit prime: no small factor, so only the strong test can reject it.
        var prime = Parse("1267650600228229401496703205653");

        Assert.True(PrimeUtils.IsPrimeNumber(prime));
        Assert.False(PrimeUtils.IsPrimeNumber(prime * prime));
    }

    [Fact]
    public void GivenNegativeBigInteger_WhenIsPrimeNumber_ThenReturnFalse()
    {
        Assert.False(PrimeUtils.IsPrimeNumber(Parse("-18446744073709551557")));
    }

    [Fact]
    public void GivenPrimeGeneratedByEveryOverload_WhenIsPrimeNumber_ThenAllOverloadsAgree()
    {
        for (var value = 0; value < 2000; value++)
        {
            var expected = PrimeUtils.IsPrimeNumber(value);

            Assert.Equal(expected, PrimeUtils.IsPrimeNumber((long)value));
            Assert.Equal(expected, PrimeUtils.IsPrimeNumber((ulong)value));
            Assert.Equal(expected, PrimeUtils.IsPrimeNumber((uint)value));
            Assert.Equal(expected, PrimeUtils.IsPrimeNumber(new BigInteger(value)));

            if (value <= short.MaxValue)
            {
                Assert.Equal(expected, PrimeUtils.IsPrimeNumber((short)value));
                Assert.Equal(expected, PrimeUtils.IsPrimeNumber((ushort)value));
            }

            if (value <= sbyte.MaxValue)
            {
                Assert.Equal(expected, PrimeUtils.IsPrimeNumber((sbyte)value));
            }

            if (value <= byte.MaxValue)
            {
                Assert.Equal(expected, PrimeUtils.IsPrimeNumber((byte)value));
            }
        }
    }

    [Theory]
    [InlineData(2047UL)] // strong pseudoprime to base 2
    [InlineData(1373653UL)] // to bases 2 and 3
    [InlineData(25326001UL)] // to bases 2, 3 and 5
    [InlineData(3215031751UL)] // to bases 2, 3, 5 and 7
    [InlineData(2152302898747UL)] // to the first five prime bases
    [InlineData(3474749660383UL)] // to the first six
    [InlineData(341550071728321UL)] // to the first eight
    [InlineData(3825123056546413051UL)] // to the first eleven
    public void GivenStrongPseudoprime_WhenIsPrimeNumber_ThenReturnFalse(ulong value)
    {
        // Each fools Miller-Rabin for a run of small bases, so only a witness set that is right for the whole
        // 64-bit range rejects it.
        Assert.False(PrimeUtils.IsPrimeNumber(value));
        Assert.False(PrimeUtils.IsPrimeNumber(new BigInteger(value)));

        if (value <= long.MaxValue)
        {
            Assert.False(PrimeUtils.IsPrimeNumber((long)value));
        }
    }

    [Theory]
    [InlineData("318665857834031151167461")] // strong pseudoprime to the first twelve prime bases
    [InlineData("3317044064679887385961981")] // to the first thirteen
    public void GivenStrongPseudoprimeBeyondSixtyFourBits_WhenIsPrimeNumber_ThenReturnFalse(string value)
    {
        Assert.False(PrimeUtils.IsPrimeNumber(Parse(value)));
    }

    [Theory]
    [InlineData(561)]
    [InlineData(1105)]
    [InlineData(1729)]
    [InlineData(41041)]
    public void GivenCarmichaelNumber_WhenIsPrimeNumber_ThenReturnFalseForEveryOverload(int value)
    {
        Assert.False(PrimeUtils.IsPrimeNumber(value));
        Assert.False(PrimeUtils.IsPrimeNumber((ulong)value));
        Assert.False(PrimeUtils.IsPrimeNumber(new BigInteger(value)));
    }

    [Fact]
    public void GivenMultipleOfASmallPrimeBeyondSixtyFourBits_WhenIsPrimeNumber_ThenReturnFalse()
    {
        // Rejected by the small-prime sieve before any Miller-Rabin round.
        var twoToTheSixtyFour = BigInteger.One << 64;

        Assert.False(PrimeUtils.IsPrimeNumber(twoToTheSixtyFour * 97 + 97));
        Assert.False(PrimeUtils.IsPrimeNumber(twoToTheSixtyFour * 3));
    }

    [Fact]
    public void GivenTheSmallestPrimesOfTheSixtyFourBitPath_WhenIsPrimeNumber_ThenReturnTrue()
    {
        // 2, 3 and 5 are settled by the small-prime check; 7 is the first candidate Miller-Rabin decides.
        Assert.All([2UL, 3UL, 5UL, 7UL], value => Assert.True(PrimeUtils.IsPrimeNumber(value)));
        Assert.All([4UL, 6UL, 15UL, 25UL, 49UL], value => Assert.False(PrimeUtils.IsPrimeNumber(value)));
    }

    [Fact]
    public void GivenLargeNonPerfectSquare_WhenBigIntegerSqrt_ThenReturnFlooredRoot()
    {
        var root = BigInteger.Pow(10, 40) + 12345;

        Assert.Equal(root, PrimeUtils.BigIntegerSqrt(root * root));
        Assert.Equal(root, PrimeUtils.BigIntegerSqrt(root * root + 2 * root));
        Assert.Equal(root - 1, PrimeUtils.BigIntegerSqrt(root * root - 1));
    }

    private static BigInteger Parse(string value) => BigInteger.Parse(value, CultureInfo.InvariantCulture);
}
