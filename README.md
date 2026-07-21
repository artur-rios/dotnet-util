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
      catch { await waiter.Wait(); }
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
  var pwd = CustomRandom.Text(new RandomStringOptions { Length = 16, IncludeSpecialCharacters = true });
  ```

- **RegularExpressions**

  ```csharp
  using ArturRios.Util.RegularExpressions;
  var isEmail = RegexCollection.Email().IsMatch("john@doe.com");
  var stripped = RegexCollection.HasNumber().Remove("abc123def"); // "abcdef"
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
