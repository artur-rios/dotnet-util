# Dotnet Util

[![Docs](https://img.shields.io/badge/docs-website-blue)](https://artur-rios.github.io/dotnet-util)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](./LICENSE)
[![NuGet](https://img.shields.io/nuget/v/ArturRios.Util.svg)](https://www.nuget.org/packages/ArturRios.Util)

Utilities for common development tasks in .NET: console output helpers, flow control (conditions, retries, and waiters), hashing (Argon2id), file I/O helpers, HTTP client helpers, math utilities, random values and strings, regex helpers, and small collections.

## Installation

```dotnet add package ArturRios.Util```

The package targets **net10.0** and depends on [`ArturRios.Output`](https://www.nuget.org/packages/ArturRios.Output), which provides result envelopes.

## Quickstart

- **Collections**

  ```csharp
  using ArturRios.Util.Collections;
  Console.WriteLine($"{AnsiColors.Green}Success!\x1b[0m");
  var pool = Characters.Digits + Characters.UpperLetters;
  ```

- **Console**

  ```csharp
  using ArturRios.Util.Console;
  CustomConsole.WriteCharLine();          // 100 dashes
  CustomConsole.WriteCharLine('=', 40);   // 40 equals signs
  ```

- **FlowControl**

  ```csharp
  using ArturRios.Util.FlowControl;
  using ArturRios.Util.FlowControl.Waiter;

  Condition.Create
    .True(user is not null).FailsWith("User is required")
    .True(emailValid).FailsWith("Invalid email")
    .ThrowIfNotSatisfied();

  Retry.New.MaxAttempts(3).DelayMilliseconds(200).Execute(() => DoFragileWork());
  var result = Retry.New.MaxAttempts(5).Execute(() => Compute());

  var waiter = new JitteredWaiter(maxRetryCount: 5);
  while (waiter.CanRetry)
  {
      try { await TryOperationAsync(); break; }
      catch { await waiter.WaitAsync(); }
  }
  ```

- **Hashing**

  ```csharp
  using ArturRios.Util.Hashing;
  var hash = Hash.EncodeWithRandomSalt("secret", out var salt);
  var ok = Hash.TextMatches("secret", hash, salt);
  ```

- **Http**

  ```csharp
  using ArturRios.Util.Http;
  var gateway = new HttpGateway(httpClient);
  var output = await gateway.GetAsync<MyResponse>("/api/items");
  if ((int)output.StatusCode == HttpStatusCodes.Ok)
      Console.WriteLine(output.Body);
  ```

- **IO**

  ```csharp
  using ArturRios.Util.IO;
  var text  = FileReader.Read(path);
  var lines = FileReader.ReadLines(path);
  var dict  = FileReader.ReadAsDictionary(path, ',');
  var obj   = FileReader.ReadAndDeserialize<MyType>(jsonPath);
  // Async variants available on FileReaderAsync
  ```

- **Math**

  ```csharp
  using ArturRios.Util.Math;
  bool isPrime = PrimeUtils.IsPrimeNumber(7919); // true
  var generator = new PrimeGenerator<int>();
  Console.WriteLine(generator.Next()); // 2
  Console.WriteLine(generator.Next()); // 3
  ```

- **Random**

  ```csharp
  using ArturRios.Util.Random;
  var n   = CustomRandom.NumberFromRng(1, 10);
  var n2  = CustomRandom.NumberFromSystemRandom(0, 100, differentFrom: 42);
  var pwd = CustomRandom.Text(new RandomStringOptions { Length = 16 }); // all sets on by default
  var pin = CustomRandom.Text(new RandomStringOptions
  {
      Length = 6, IncludeLowercase = false, IncludeUppercase = false, IncludeSpecialCharacters = false
  });
  ```

- **RegularExpressions**

  ```csharp
  using ArturRios.Util.RegularExpressions;
  var isEmail = RegexCollection.Email().IsMatch("john@doe.com");
  var stripped = RegexCollection.HasNumber().Remove("abc123def"); // "abcdef"
  ```

- **Text**

  ```csharp
  using ArturRios.Util.Text;
  var missing  = "ab1".Missing(CharacterClasses.Upper | CharacterClasses.Special); // Upper | Special
  var ok       = EmailAddress.TryNormalize("MA@Hostname.COM", out var address);    // "MA@hostname.com"
  ```

## Documentation

Full API reference, class diagrams, and usage examples:

- [Collections](https://artur-rios.github.io/dotnet-util/collections/)
- [Console](https://artur-rios.github.io/dotnet-util/console/)
- [Flow Control](https://artur-rios.github.io/dotnet-util/flow-control/)
- [Hashing](https://artur-rios.github.io/dotnet-util/hashing/)
- [Http](https://artur-rios.github.io/dotnet-util/http/)
- [IO](https://artur-rios.github.io/dotnet-util/io/)
- [Math](https://artur-rios.github.io/dotnet-util/math/)
- [Random](https://artur-rios.github.io/dotnet-util/random/)
- [Regular Expressions](https://artur-rios.github.io/dotnet-util/regular-expressions/)
- [Text](https://artur-rios.github.io/dotnet-util/text/)

## Upgrading to 2.0

2.0 fixes several correctness bugs. Most call sites need no change, but the following behave differently.

**Correctness fixes that change results**

- `PrimeUtils.IsPrimeNumber(long)` rejects negative values. It previously reinterpreted the bits as
  `ulong`, so `IsPrimeNumber(-59L)` returned `true`.
- `PrimeUtils.IsPrimeNumber(BigInteger)` uses Miller-Rabin above 2^64 instead of trial division, which
  never returned for a large operand. Below 2^64 the answer is still exact; above it, a "prime" verdict is
  probabilistic with an error probability below 4^-40.
- `Retry.MaxAttempts(n)` now means *n total executions*. It previously ran `n + 1` times, and consumed its
  own configuration, so a reused instance had no attempts left. Add one to your argument to keep the old
  execution count.
- `CustomRandom.NumberFromSystemRandom` treats `end` as **inclusive**, matching `NumberFromRng`. Pass
  `end - 1` to keep the old exclusive behavior.
- `Characters.Special` gained the backtick, tilde and backslash, completing the ASCII punctuation set. This
  changes the alphabet `CustomRandom.Text` draws from and what `HasSpecialChar` reports.
- `HttpOutput` and `HttpGateway` serialize with `System.Text.Json` instead of `Newtonsoft.Json`; property
  matching on deserialization stays case insensitive. The `Newtonsoft.Json` dependency is gone.
- `Condition` reports duplicate failure messages once per failing condition instead of collapsing them, and
  `FailsWith` throws `InvalidOperationException` when no `True`/`False` precedes it.

**Signature and type changes**

- `JitteredWaiter.Wait()` is **removed**; use `WaitAsync(CancellationToken)`. Waits are now capped at
  `maxWaitMilliseconds` (30 s by default) rather than overflowing past ~20 retries.
- `HttpOutput.ReadContent()` is **removed**; use `ReadContentAsync(CancellationToken)`. `StatusCode`,
  `Headers` and `Body` are read-only, and `ContentHeaders`, `RawBody` and `IsSuccess` are new.
- `HttpStatusCodes` groups are `ImmutableArray<int>` rather than `int[]`.
- `PrimeGenerator<T>` is constrained to `IBinaryInteger<T>`. An unsupported `T` is now a compile error
  instead of a constructor `ArgumentException`.
- `ConditionFailedException.Errors` is a property rather than a public field.
- `FileReaderAsync` methods take an optional `CancellationToken`.

**Newly enforced validation**

- `Retry.MaxAttempts` and `DelayMilliseconds`, `JitteredWaiter`'s constructor, and `HashConfiguration`'s
  cost parameters all reject out-of-range values.
- `Hash` rejects empty text and salts shorter than 8 bytes, and `Hash.TextMatches` compares in constant
  time and accepts the `HashConfiguration` the hash was produced with.
- `CustomRandom` rejects an inverted range, and a single-value range equal to `differentFrom`, instead of
  looping forever. `CustomRandom.Text` gives up with `InvalidOperationException` when `differentFrom`
  excludes everything it can produce.
- `ReadAsDictionary` throws on duplicate header names instead of silently dropping a column.

## Versioning

Semantic Versioning (SemVer). Breaking changes result in a new major version. New methods or non-breaking behavior
changes increment the minor version; fixes or tweaks increment the patch.

## Build, test and publish

Use the official [.NET CLI](https://learn.microsoft.com/en-us/dotnet/core/tools/) to build, test and publish the project and Git for source control.
If you want, optional helper toolsets I built to facilitate these tasks are available:

- [Dotnet Tools](https://github.com/artur-rios/dotnet-tools)
- [Python Dotnet Tools](https://github.com/artur-rios/python-dotnet-tools)

## Legal Details

This project is licensed under the [MIT License](https://en.wikipedia.org/wiki/MIT_License). A copy of the license is available at [LICENSE](./LICENSE) in the repository.
