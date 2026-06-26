using System.Numerics;

namespace ArturRios.Util.Math;

/// <summary>
/// Static convenience wrapper that shares a single thread-safe generator.
/// </summary>
public static class PrimeUtils
{
    /// <summary>
    /// Determines whether the specified 8-bit signed integer is prime.
    /// </summary>
    /// <param name="value">The value to test for primality.</param>
    /// <returns>True if <paramref name="value"/> is prime; otherwise false.</returns>
    public static bool IsPrimeNumber(sbyte value)
    {
        return IsPrimeNumber((int)value);
    }

    /// <summary>
    /// Determines whether the specified 8-bit unsigned integer is prime.
    /// </summary>
    /// <param name="value">The value to test for primality.</param>
    /// <returns>True if <paramref name="value"/> is prime; otherwise false.</returns>
    public static bool IsPrimeNumber(byte value)
    {
        return IsPrimeNumber((int)value);
    }

    /// <summary>
    /// Determines whether the specified 16-bit signed integer is prime.
    /// </summary>
    /// <param name="value">The value to test for primality.</param>
    /// <returns>True if <paramref name="value"/> is prime; otherwise false.</returns>
    public static bool IsPrimeNumber(short value)
    {
        return IsPrimeNumber((int)value);
    }

    /// <summary>
    /// Determines whether the specified 16-bit unsigned integer is prime.
    /// </summary>
    /// <param name="value">The value to test for primality.</param>
    /// <returns>True if <paramref name="value"/> is prime; otherwise false.</returns>
    public static bool IsPrimeNumber(ushort value)
    {
        return IsPrimeNumber((int)value);
    }

    /// <summary>
    /// Determines whether the specified 32-bit unsigned integer is prime.
    /// </summary>
    /// <param name="value">The value to test for primality.</param>
    /// <returns>True if <paramref name="value"/> is prime; otherwise false.</returns>
    public static bool IsPrimeNumber(uint value)
    {
        return value <= int.MaxValue ? IsPrimeNumber((int)value) : IsPrimeNumber((ulong)value);
    }

    /// <summary>
    /// Optimized primality check for 32-bit integers using trial division.
    /// </summary>
    /// <param name="value">The value to test for primality.</param>
    /// <returns>True if <paramref name="value"/> is prime; otherwise false.</returns>
    public static bool IsPrimeNumber(int value)
    {
        if (value < 2)
        {
            return false;
        }

        if (value == 2)
        {
            return true;
        }

        if (value % 2 == 0)
        {
            return false;
        }

        var limit = (int)System.Math.Sqrt(value);

        for (var divisor = 3; divisor <= limit; divisor += 2)
        {
            if (value % divisor == 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Optimized primality check for 64-bit integers using deterministic Miller-Rabin for 64-bit range.
    /// </summary>
    /// <param name="value">The value to test for primality.</param>
    /// <returns>True if <paramref name="value"/> is prime; otherwise false.</returns>
    public static bool IsPrimeNumber(long value)
    {
        var candidate = (ulong)value;
        var smallPrimeResult = CheckSmallPrimesAndDivisors(candidate);

        if (smallPrimeResult.HasValue)
        {
            return smallPrimeResult.Value;
        }

        ulong[] witnesses = [2UL, 325UL, 9375UL, 28178UL, 450775UL, 9780504UL, 1795265022UL];

        var d = candidate - 1UL;
        var s = 0;

        while ((d & 1UL) == 0UL)
        {
            d >>= 1;
            s++;
        }

        var candidateBigInteger = new BigInteger(candidate);

        foreach (var witnessValue in witnesses)
        {
            if (witnessValue % candidate == 0UL)
            {
                return true;
            }

            var witnessBigInteger = new BigInteger(witnessValue % candidate);
            var exponentBigInteger = new BigInteger(d);
            var x = BigInteger.ModPow(witnessBigInteger, exponentBigInteger, candidateBigInteger);

            if (x == BigInteger.One || x == candidateBigInteger - BigInteger.One)
            {
                continue;
            }

            var roundPassed = false;
            for (var iteration = 1; iteration < s; iteration++)
            {
                x = x * x % candidateBigInteger;

                if (x == candidateBigInteger - BigInteger.One)
                {
                    roundPassed = true;

                    break;
                }
            }

            if (!roundPassed)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified 64-bit unsigned integer is prime.
    /// </summary>
    /// <param name="value">The value to test for primality.</param>
    /// <returns>True if <paramref name="value"/> is prime; otherwise false.</returns>
    public static bool IsPrimeNumber(ulong value)
    {
        var smallPrimeResult = CheckSmallPrimesAndDivisors(value);
        if (smallPrimeResult.HasValue)
        {
            return smallPrimeResult.Value;
        }

        ulong[] witnesses = [2UL, 325UL, 9375UL, 28178UL, 450775UL, 9780504UL, 1795265022UL];

        var d = value - 1UL;
        var s = 0;
        while ((d & 1UL) == 0UL)
        {
            d >>= 1;
            s++;
        }

        var candidateBigInteger = new BigInteger(value);

        foreach (var witnessValue in witnesses)
        {
            if (witnessValue % value == 0UL)
            {
                return true;
            }

            var witnessBigInteger = new BigInteger(witnessValue % value);
            var exponentBigInteger = new BigInteger(d);

            var x = BigInteger.ModPow(witnessBigInteger, exponentBigInteger, candidateBigInteger);

            if (x == BigInteger.One || x == candidateBigInteger - BigInteger.One)
            {
                continue;
            }

            var roundPassed = false;

            for (var iteration = 1; iteration < s; iteration++)
            {
                x = (x * x) % candidateBigInteger;

                if (x == candidateBigInteger - BigInteger.One)
                {
                    roundPassed = true;
                    break;
                }
            }

            if (!roundPassed)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified BigInteger is a prime number
    /// </summary>
    /// <param name="value">The value to test for primality.</param>
    /// <returns>True if <paramref name="value"/> is prime; otherwise false.</returns>
    public static bool IsPrimeNumber(BigInteger value)
    {
        // Numbers less than 2 are not prime
        if (value < 2)
        {
            return false;
        }

        if (value == 2)
        {
            return true;
        }

        if (value % 2 == 0)
        {
            return false;
        }

        var limit = BigIntegerSqrt(value);

        for (BigInteger divisor = 3; divisor <= limit; divisor += 2)
        {
            if (value % divisor == 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Computes the integer square root (floor) of the specified non-negative <see cref="BigInteger"/>.
    /// </summary>
    /// <param name="n">The non-negative value whose integer square root is to be computed.</param>
    /// <returns>The greatest integer less than or equal to the square root of <paramref name="n"/>.</returns>
    public static BigInteger BigIntegerSqrt(BigInteger n)
    {
        if (n < 0)
        {
            throw new ArithmeticException("Cannot compute square root of a negative number.");
        }

        if (n == 0)
        {
            return 0;
        }

        BigInteger low = 1;

        var high = n;

        while (low <= high)
        {
            var mid = (low + high) >> 1; // divide by 2
            var square = mid * mid;
            var compareResult = square.CompareTo(n);

            if (compareResult == 0)
            {
                return mid;
            }

            if (compareResult < 0)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return high;
    }

    /// <summary>
    /// Checks if a number is a small prime (2, 3, 5) or divisible by small primes.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns>True if prime (2, 3, or 5), false if composite (divisible by 2, 3, or 5, or less than 2), null to continue with advanced primality test.</returns>
    private static bool? CheckSmallPrimesAndDivisors(ulong value)
    {
        if (value < 2UL)
        {
            return false;
        }

        if (value is 2UL or 3UL or 5UL)
        {
            return true;
        }

        if ((value & 1UL) == 0UL || value % 3UL == 0UL || value % 5UL == 0UL)
        {
            return false;
        }

        return null;
    }
}
