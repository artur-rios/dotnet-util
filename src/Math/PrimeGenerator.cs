using System.Numerics;

namespace ArturRios.Util.Math;

/// <summary>
/// A generic prime number generator that yields the primes of an integer type in ascending order.
/// Create an instance to get an independent sequence of primes.
/// </summary>
/// <typeparam name="T">
/// The integer type to generate primes for. Any <see cref="IBinaryInteger{TSelf}"/> works, which covers
/// sbyte, byte, short, ushort, int, uint, long, ulong, nint, nuint, Int128, UInt128 and
/// <see cref="BigInteger"/>. Types that cannot represent integers, such as double or decimal, are
/// rejected by the compiler rather than at run time.
/// </typeparam>
public class PrimeGenerator<T> where T : IBinaryInteger<T>
{
    private readonly Lock _lock = new();
    private T _current = T.One;

    /// <summary>
    /// Returns the next prime in the sequence (2, 3, 5, 7, ...).
    /// Thread-safe.
    /// </summary>
    /// <returns>The next prime number of type T.</returns>
    /// <exception cref="OverflowException">
    /// Thrown when the sequence runs past the largest prime representable by <typeparamref name="T"/>.
    /// The generator is left on its last prime, so a subsequent call throws again rather than wrapping.
    /// </exception>
    public T Next()
    {
        lock (_lock)
        {
            var two = T.One + T.One;
            var candidate = Increment(_current);

            if (candidate < two)
            {
                candidate = two;
            }

            while (!IsPrime(candidate))
            {
                candidate = Increment(candidate);
            }

            _current = candidate;

            return candidate;
        }
    }

    /// <summary>
    /// Adds one, surfacing an <see cref="OverflowException"/> instead of wrapping around.
    /// </summary>
    /// <remarks>
    /// The checked operator is a no-op for <see cref="BigInteger"/>, which never overflows, and throws for
    /// every fixed-width type.
    /// </remarks>
    private static T Increment(T value) => checked(value + T.One);

    /// <summary>
    /// Tests a candidate using the narrowest <see cref="PrimeUtils"/> overload that can hold it.
    /// </summary>
    private static bool IsPrime(T candidate)
    {
        if (T.IsNegative(candidate))
        {
            return false;
        }

        // A non-negative value of eight bytes or fewer always fits in a ulong, sign bit included.
        return candidate.GetByteCount() <= sizeof(ulong)
            ? PrimeUtils.IsPrimeNumber(ulong.CreateTruncating(candidate))
            : PrimeUtils.IsPrimeNumber(BigInteger.CreateChecked(candidate));
    }
}
