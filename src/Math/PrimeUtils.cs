using System.Numerics;

namespace ArturRios.Util.Math;

/// <summary>
/// Stateless primality helpers for every built-in integer type and <see cref="BigInteger"/>.
/// </summary>
/// <remarks>
/// <para>
/// Small operands are settled by trial division. Everything that fits in 64 bits is settled by
/// Miller-Rabin with the witness set that is deterministic below 2^64, so the answer is exact.
/// Above 2^64 the test is probabilistic: see <see cref="IsPrimeNumber(BigInteger)"/>.
/// </para>
/// <para>
/// Every overload is pure and therefore safe to call from multiple threads. Use
/// <see cref="PrimeGenerator{T}"/> when a sequence of primes is needed rather than a single test.
/// </para>
/// </remarks>
public static class PrimeUtils
{
    /// <summary>
    /// Witness set that makes Miller-Rabin deterministic for every value below 2^64.
    /// </summary>
    private static readonly ulong[] DeterministicWitnesses =
        [2UL, 325UL, 9375UL, 28178UL, 450775UL, 9780504UL, 1795265022UL];

    /// <summary>
    /// The first primes, used to strip most composites before the expensive test runs.
    /// </summary>
    private static readonly int[] SmallPrimes =
        [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97];

    /// <summary>
    /// Number of random Miller-Rabin rounds applied to candidates above 2^64, where no
    /// deterministic witness set is known. Each round leaves at most a 1-in-4 chance of accepting a
    /// composite, so the overall error probability is below 4^-<see cref="ProbabilisticRounds"/>.
    /// </summary>
    private const int ProbabilisticRounds = 40;

    /// <summary>
    /// Determines whether the specified 8-bit signed integer is prime.
    /// </summary>
    /// <param name="value">The value to test for primality.</param>
    /// <returns>True if <paramref name="value"/> is prime; otherwise false. Negative values are never prime.</returns>
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
    /// <returns>True if <paramref name="value"/> is prime; otherwise false. Negative values are never prime.</returns>
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
    /// <returns>True if <paramref name="value"/> is prime; otherwise false. Negative values are never prime.</returns>
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
    /// <returns>True if <paramref name="value"/> is prime; otherwise false. Negative values are never prime.</returns>
    /// <remarks>
    /// A negative value is rejected before the unsigned test runs. Reinterpreting its bits as a
    /// <see cref="ulong"/> would test a completely unrelated number: -59, for instance, would be tested
    /// as 2^64-59, which is prime.
    /// </remarks>
    public static bool IsPrimeNumber(long value)
    {
        return value >= 0L && IsPrimeNumber((ulong)value);
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

        return IsStrongProbablePrime(new BigInteger(value), DeterministicWitnesses.Select(witness => new BigInteger(witness)));
    }

    /// <summary>
    /// Determines whether the specified <see cref="BigInteger"/> is a prime number.
    /// </summary>
    /// <param name="value">The value to test for primality.</param>
    /// <returns>True if <paramref name="value"/> is prime; otherwise false. Negative values are never prime.</returns>
    /// <remarks>
    /// Below 2^64 the answer is exact, because a deterministic Miller-Rabin witness set is known for that
    /// range. Above 2^64 the answer is probabilistic: <see cref="ProbabilisticRounds"/> random rounds are
    /// applied, leaving a false-positive probability below 4^-40, which is far smaller than the odds of a
    /// hardware fault. A "false" result is always exact. Use a certifying test such as ECPP when a proof
    /// rather than a very strong probability is required.
    /// </remarks>
    public static bool IsPrimeNumber(BigInteger value)
    {
        if (value < 2)
        {
            return false;
        }

        if (value <= ulong.MaxValue)
        {
            return IsPrimeNumber((ulong)value);
        }

        foreach (var smallPrime in SmallPrimes)
        {
            if (value % smallPrime == 0)
            {
                return false;
            }
        }

        return IsStrongProbablePrime(value, RandomWitnesses(value, ProbabilisticRounds));
    }

    /// <summary>
    /// Computes the integer square root (floor) of the specified non-negative <see cref="BigInteger"/>.
    /// </summary>
    /// <param name="n">The non-negative value whose integer square root is to be computed.</param>
    /// <returns>The greatest integer less than or equal to the square root of <paramref name="n"/>.</returns>
    /// <exception cref="ArithmeticException"><paramref name="n"/> is negative.</exception>
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
    /// Runs the Miller-Rabin strong probable prime test against every supplied witness.
    /// </summary>
    /// <param name="value">The candidate, which must be odd and greater than the largest small prime.</param>
    /// <param name="witnesses">The bases to test against.</param>
    /// <returns>True when the candidate survives every witness.</returns>
    private static bool IsStrongProbablePrime(BigInteger value, IEnumerable<BigInteger> witnesses)
    {
        var lastValue = value - BigInteger.One;
        var d = lastValue;
        var s = 0;

        while (d.IsEven)
        {
            d >>= 1;
            s++;
        }

        foreach (var witness in witnesses)
        {
            var normalizedWitness = witness % value;

            // A witness that is a multiple of the candidate carries no information; skip it.
            if (normalizedWitness.IsZero)
            {
                continue;
            }

            var x = BigInteger.ModPow(normalizedWitness, d, value);

            if (x.IsOne || x == lastValue)
            {
                continue;
            }

            var roundPassed = false;

            for (var iteration = 1; iteration < s; iteration++)
            {
                x = x * x % value;

                if (x == lastValue)
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
    /// Produces uniformly distributed Miller-Rabin bases in the range [2, <paramref name="value"/> - 2].
    /// </summary>
    /// <param name="value">The candidate being tested.</param>
    /// <param name="count">How many bases to produce.</param>
    private static IEnumerable<BigInteger> RandomWitnesses(BigInteger value, int count)
    {
        // The candidate has already been proven larger than 2^64, so the range is never degenerate.
        var range = value - 3;
        var byteCount = range.GetByteCount(true);

        for (var i = 0; i < count; i++)
        {
            var buffer = new byte[byteCount + 1];

            System.Random.Shared.NextBytes(buffer.AsSpan(0, byteCount));
            buffer[byteCount] = 0; // Force a non-negative interpretation.

            yield return new BigInteger(buffer) % range + 2;
        }
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
