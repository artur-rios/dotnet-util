using System.Numerics;
using ArturRios.Util.Math;

namespace ArturRios.Util.Tests.Math;

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
        var bigInteger = BigInteger.Parse(value);

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
        var result = PrimeUtils.BigIntegerSqrt(BigInteger.Parse(value));

        Assert.Equal(BigInteger.Parse(expected), result);
    }

    [Theory]
    [InlineData("2", "1")]
    [InlineData("3", "1")]
    [InlineData("15", "3")]
    [InlineData("24", "4")]
    [InlineData("99", "9")]
    public void GivenNonPerfectSquare_WhenBigIntegerSqrt_ThenReturnFlooredRoot(string value, string expected)
    {
        var result = PrimeUtils.BigIntegerSqrt(BigInteger.Parse(value));

        Assert.Equal(BigInteger.Parse(expected), result);
    }

    [Fact]
    public void GivenNegativeValue_WhenBigIntegerSqrt_ThenThrowArithmeticException()
    {
        Assert.Throws<ArithmeticException>(() => PrimeUtils.BigIntegerSqrt(BigInteger.MinusOne));
    }
}
