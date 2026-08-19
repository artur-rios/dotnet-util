---
title: Documentation
linkTitle: Documentation
weight: 20
description: >-
  Utilities for common development tasks in .NET: console output helpers, flow control (conditions, retries, and waiters), hashing (Argon2id), file I/O helpers...
---

Utilities for common development tasks in .NET: console output helpers, flow control (conditions, retries, and waiters), hashing (Argon2id), file I/O helpers, HTTP client helpers, math utilities, random values and strings, regex helpers, text and email helpers, and small collections.

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
  var pwd = CustomRandom.Text(new RandomStringOptions { Length = 16 }); // all sets on by default
  var pin = CustomRandom.Text(new RandomStringOptions
  {
      Length = 6, IncludeLowercase = false, IncludeUppercase = false, IncludeSpecialCharacters = false
  });
  ```

- **RegularExpressions**

  ```csharp
  using ArturRios.Util.RegularExpressions;
  var isEmail  = RegexCollection.Email().IsMatch("john@doe.com");
  var stripped = RegexCollection.HasNumber().Remove("abc123def"); // "abcdef"
  ```

- **Text**

  ```csharp
  using ArturRios.Util.Text;
  var missing  = "ab1".Missing(CharacterClasses.Upper | CharacterClasses.Special); // Upper | Special
  var ok       = EmailAddress.TryNormalize("MA@Hostname.COM", out var address);    // "MA@hostname.com"
  ```

## Detailed documentation

Full API reference, class diagrams, and usage examples:

- [Collections](collections/)
- [Console](console/)
- [Flow Control](flow-control/)
- [Hashing](hashing/)
- [Http](http/)
- [IO](io/)
- [Math](math/)
- [Random](random/)
- [Regular Expressions](regular-expressions/)
- [Text](text/)

## Versioning

Semantic Versioning (SemVer). Breaking changes result in a new major version. New methods or non-breaking behavior
changes increment the minor version; fixes or tweaks increment the patch.

## Build, test and publish

Use the official [.NET CLI](https://learn.microsoft.com/en-us/dotnet/core/tools/) to build, test and publish the project and Git for source control.
If you want, optional helper toolsets I built to facilitate these tasks are available:

- [Dotnet Tools](https://github.com/artur-rios/dotnet-tools)
- [Python Dotnet Tools](https://github.com/artur-rios/python-dotnet-tools)

## Legal Details

This project is licensed under the [MIT License](https://en.wikipedia.org/wiki/MIT_License). A copy of the license is available at [LICENSE](https://github.com/artur-rios/dotnet-util/blob/main/LICENSE) in the repository.
