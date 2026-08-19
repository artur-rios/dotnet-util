---
title: Math
weight: 70
description: >-
  bool a = PrimeUtils.IsPrimeNumber(7919); // true bool b = PrimeUtils.IsPrimeNumber(7920); // false bool c = PrimeUtils.IsPrimeNumber(1_000_000_007); // true...
---

## Features

- `PrimeUtils`: static primality tests for all standard integer types (`sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`) and `BigInteger`. Uses trial division for 32-bit values and deterministic Miller-Rabin witnesses up to 2^64, where the answer is exact. Beyond 2^64 a `BigInteger` is settled by 40 random Miller-Rabin rounds, so a "prime" verdict is probabilistic (error below 4^-40) while a "not prime" verdict is exact. Negative values are never prime. Also exposes `BigIntegerSqrt` for computing the integer (floor) square root of a `BigInteger`.
- `PrimeGenerator<T>`: generates an ascending, infinite sequence of prime numbers of the chosen integer type. Thread-safe; each instance maintains its own independent counter. `T` is constrained to `IBinaryInteger<T>`, so `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `nint`, `nuint`, `Int128`, `UInt128` and `BigInteger` all work, and anything else is a compile error rather than a run-time exception. Running past the largest prime a fixed-width `T` can hold throws `OverflowException`.

## Class Diagram

```mermaid
classDiagram
    namespace Math {
        class PrimeUtils {
            <<static>>
            +bool IsPrimeNumber(sbyte value)
            +bool IsPrimeNumber(byte value)
            +bool IsPrimeNumber(short value)
            +bool IsPrimeNumber(ushort value)
            +bool IsPrimeNumber(int value)
            +bool IsPrimeNumber(uint value)
            +bool IsPrimeNumber(long value)
            +bool IsPrimeNumber(ulong value)
            +bool IsPrimeNumber(BigInteger value)
            +BigInteger BigIntegerSqrt(BigInteger n)
        }
        class PrimeGenerator~T~ {
            -T _current
            -Lock _lock
            +T Next()
        }
    }
    PrimeGenerator~T~ ..> PrimeUtils : uses
```

## Usage

### Primality checks

```csharp
using ArturRios.Util.Math;

bool a = PrimeUtils.IsPrimeNumber(7919);                  // true
bool b = PrimeUtils.IsPrimeNumber(7920);                  // false
bool c = PrimeUtils.IsPrimeNumber(1_000_000_007);         // true
bool d = PrimeUtils.IsPrimeNumber(9_223_372_036_854_775_783L); // true (near long.MaxValue)
bool e = PrimeUtils.IsPrimeNumber(new BigInteger(999983));// true
```

### Generating primes in sequence

```csharp
using ArturRios.Util.Math;

var generator = new PrimeGenerator<int>();

Console.WriteLine(generator.Next()); // 2
Console.WriteLine(generator.Next()); // 3
Console.WriteLine(generator.Next()); // 5
Console.WriteLine(generator.Next()); // 7
```

### Using other integer types

```csharp
// byte — sequence ends when next prime would exceed 255
var byteGen = new PrimeGenerator<byte>();
byte first = byteGen.Next(); // 2

// BigInteger — unbounded
var bigGen = new PrimeGenerator<BigInteger>();
BigInteger p = bigGen.Next(); // 2
```

### Integer square root

```csharp
BigInteger root  = PrimeUtils.BigIntegerSqrt(new BigInteger(99));  // 9  (floor)
BigInteger exact = PrimeUtils.BigIntegerSqrt(new BigInteger(100)); // 10 (exact)
```

### Thread-safe independent generators

```csharp
var g1 = new PrimeGenerator<int>();
var g2 = new PrimeGenerator<int>();

g1.Next(); // 2
g1.Next(); // 3

g2.Next(); // 2  ← independent — starts from the beginning
g2.Next(); // 3
```
