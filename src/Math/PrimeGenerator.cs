using System.Numerics;

namespace ArturRios.Util.Math;

/// <summary>
/// A generic prime number generator that generates primes of a specified integer type.
/// Create an instance to get an independent sequence of primes.
/// </summary>
/// <typeparam name="T">The integer type to generate primes for. Supported types: sbyte, byte, short, ushort, int, uint, long, ulong, or BigInteger.</typeparam>
public class PrimeGenerator<T> where T : struct
{
    private T _current;
    private readonly Lock _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimeGenerator{T}"/> class.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if T is not a supported integer type.</exception>
    public PrimeGenerator()
    {
        ValidateSupportedType();
        _current = GetStartValue();
    }

    /// <summary>
    /// Returns the next prime in the sequence (2, 3, 5, 7, ...).
    /// Thread-safe.
    /// </summary>
    /// <returns>The next prime number of type T.</returns>
    /// <exception cref="OverflowException">Thrown when the next prime exceeds the maximum value for type T.</exception>
    public T Next()
    {
        lock (_lock)
        {
            var candidate = Increment(_current);
            EnsureAtLeastTwo(ref candidate);

            while (!IsPrime(candidate))
            {
                candidate = Increment(candidate);
            }

            _current = candidate;
            return candidate;
        }
    }

    /// <summary>
    /// Validates that T is one of the supported integer types.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if T is not a supported type.</exception>
    private static void ValidateSupportedType()
    {
        var typeOfT = typeof(T);
        if (typeOfT != typeof(sbyte) && typeOfT != typeof(byte) &&
            typeOfT != typeof(short) && typeOfT != typeof(ushort) &&
            typeOfT != typeof(int) && typeOfT != typeof(uint) &&
            typeOfT != typeof(long) && typeOfT != typeof(ulong) &&
            typeOfT != typeof(BigInteger))
        {
            throw new ArgumentException(
                $"Type '{typeof(T).Name}' is not supported for PrimeGenerator. " +
                $"Supported types are: sbyte, byte, short, ushort, int, uint, long, ulong, and BigInteger.");
        }
    }

    /// <summary>
    /// Gets the starting value (one less than the first prime).
    /// </summary>
    private static T GetStartValue()
    {
        if (typeof(T) == typeof(sbyte))
        {
            return (T)(object)(sbyte)1;
        }

        if (typeof(T) == typeof(byte))
        {
            return (T)(object)(byte)1;
        }

        if (typeof(T) == typeof(short))
        {
            return (T)(object)(short)1;
        }

        if (typeof(T) == typeof(ushort))
        {
            return (T)(object)(ushort)1;
        }

        if (typeof(T) == typeof(int))
        {
            return (T)(object)1;
        }

        if (typeof(T) == typeof(uint))
        {
            return (T)(object)1U;
        }

        if (typeof(T) == typeof(long))
        {
            return (T)(object)1L;
        }

        if (typeof(T) == typeof(ulong))
        {
            return (T)(object)1UL;
        }

        if (typeof(T) == typeof(BigInteger))
        {
            return (T)(object)BigInteger.One;
        }

        throw new InvalidOperationException("Unexpected type in GetStartValue");
    }

    /// <summary>
    /// Increments the given value by one, throwing on overflow.
    /// </summary>
    private static T Increment(T value)
    {
        if (typeof(T) == typeof(sbyte))
        {
            var sbyteValue = (sbyte)(object)value;
            if (sbyteValue == sbyte.MaxValue)
            {
                throw new OverflowException($"Cannot increment beyond maximum value for {typeof(T).Name}");
            }

            return (T)(object)(sbyte)(sbyteValue + 1);
        }

        if (typeof(T) == typeof(byte))
        {
            var byteValue = (byte)(object)value;
            if (byteValue == byte.MaxValue)
            {
                throw new OverflowException($"Cannot increment beyond maximum value for {typeof(T).Name}");
            }

            return (T)(object)(byte)(byteValue + 1);
        }

        if (typeof(T) == typeof(short))
        {
            var shortValue = (short)(object)value;
            if (shortValue == short.MaxValue)
            {
                throw new OverflowException($"Cannot increment beyond maximum value for {typeof(T).Name}");
            }

            return (T)(object)(short)(shortValue + 1);
        }

        if (typeof(T) == typeof(ushort))
        {
            var ushortValue = (ushort)(object)value;
            if (ushortValue == ushort.MaxValue)
            {
                throw new OverflowException($"Cannot increment beyond maximum value for {typeof(T).Name}");
            }

            return (T)(object)(ushort)(ushortValue + 1);
        }

        if (typeof(T) == typeof(int))
        {
            var intValue = (int)(object)value;
            if (intValue == int.MaxValue)
            {
                throw new OverflowException($"Cannot increment beyond maximum value for {typeof(T).Name}");
            }

            return (T)(object)(intValue + 1);
        }

        if (typeof(T) == typeof(uint))
        {
            var uintValue = (uint)(object)value;
            if (uintValue == uint.MaxValue)
            {
                throw new OverflowException($"Cannot increment beyond maximum value for {typeof(T).Name}");
            }

            return (T)(object)(uintValue + 1U);
        }

        if (typeof(T) == typeof(long))
        {
            var longValue = (long)(object)value;
            if (longValue == long.MaxValue)
            {
                throw new OverflowException($"Cannot increment beyond maximum value for {typeof(T).Name}");
            }

            return (T)(object)(longValue + 1L);
        }

        if (typeof(T) == typeof(ulong))
        {
            var ulongValue = (ulong)(object)value;
            if (ulongValue == ulong.MaxValue)
            {
                throw new OverflowException($"Cannot increment beyond maximum value for {typeof(T).Name}");
            }

            return (T)(object)(ulongValue + 1UL);
        }

        if (typeof(T) == typeof(BigInteger))
        {
            var bigIntegerValue = (BigInteger)(object)value;
            return (T)(object)(bigIntegerValue + BigInteger.One);
        }

        throw new InvalidOperationException("Unexpected type in Increment");
    }

    /// <summary>
    /// Ensures the candidate is at least 2 (the first prime).
    /// </summary>
    private static void EnsureAtLeastTwo(ref T candidate)
    {
        if (typeof(T) == typeof(sbyte))
        {
            var sbyteValue = (sbyte)(object)candidate;
            if (sbyteValue < 2)
            {
                candidate = (T)(object)(sbyte)2;
            }
        }
        else if (typeof(T) == typeof(byte))
        {
            var byteValue = (byte)(object)candidate;
            if (byteValue < 2)
            {
                candidate = (T)(object)(byte)2;
            }
        }
        else if (typeof(T) == typeof(short))
        {
            var shortValue = (short)(object)candidate;
            if (shortValue < 2)
            {
                candidate = (T)(object)(short)2;
            }
        }
        else if (typeof(T) == typeof(ushort))
        {
            var ushortValue = (ushort)(object)candidate;
            if (ushortValue < 2)
            {
                candidate = (T)(object)(ushort)2;
            }
        }
        else if (typeof(T) == typeof(int))
        {
            var intValue = (int)(object)candidate;
            if (intValue < 2)
            {
                candidate = (T)(object)2;
            }
        }
        else if (typeof(T) == typeof(uint))
        {
            var uintValue = (uint)(object)candidate;
            if (uintValue < 2)
            {
                candidate = (T)(object)2U;
            }
        }
        else if (typeof(T) == typeof(long))
        {
            var longValue = (long)(object)candidate;
            if (longValue < 2)
            {
                candidate = (T)(object)2L;
            }
        }
        else if (typeof(T) == typeof(ulong))
        {
            var ulongValue = (ulong)(object)candidate;
            if (ulongValue < 2)
            {
                candidate = (T)(object)2UL;
            }
        }
        else if (typeof(T) == typeof(BigInteger))
        {
            var bigIntegerValue = (BigInteger)(object)candidate;
            if (bigIntegerValue < BigInteger.One + BigInteger.One)
            {
                candidate = (T)(object)(BigInteger.One + BigInteger.One);
            }
        }
    }

    /// <summary>
    /// Tests if the candidate is prime using PrimeUtils.IsPrimeNumber.
    /// </summary>
    private static bool IsPrime(T candidate)
    {
        if (typeof(T) == typeof(sbyte))
        {
            return PrimeUtils.IsPrimeNumber((sbyte)(object)candidate);
        }

        if (typeof(T) == typeof(byte))
        {
            return PrimeUtils.IsPrimeNumber((byte)(object)candidate);
        }

        if (typeof(T) == typeof(short))
        {
            return PrimeUtils.IsPrimeNumber((short)(object)candidate);
        }

        if (typeof(T) == typeof(ushort))
        {
            return PrimeUtils.IsPrimeNumber((ushort)(object)candidate);
        }

        if (typeof(T) == typeof(int))
        {
            return PrimeUtils.IsPrimeNumber((int)(object)candidate);
        }

        if (typeof(T) == typeof(uint))
        {
            return PrimeUtils.IsPrimeNumber((uint)(object)candidate);
        }

        if (typeof(T) == typeof(long))
        {
            return PrimeUtils.IsPrimeNumber((long)(object)candidate);
        }

        if (typeof(T) == typeof(ulong))
        {
            return PrimeUtils.IsPrimeNumber((ulong)(object)candidate);
        }

        if (typeof(T) == typeof(BigInteger))
        {
            return PrimeUtils.IsPrimeNumber((BigInteger)(object)candidate);
        }

        throw new InvalidOperationException("Unexpected type in IsPrime");
    }
}
