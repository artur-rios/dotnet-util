# Changelog

All notable changes to `ArturRios.Util` are recorded in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `Condition.BlankFailureMessage`, the error `ToProcessOutput` reports for a condition that failed with a blank
  message.

### Changed

- `FileReader.ReadAndDeserialize<T>` and `FileReaderAsync.ReadAndDeserializeAsync<T>` match JSON property names
  regardless of case, as `HttpOutput<TBody>` already did. A camelCase file used to bind every member to its default
  without any error, so the same document read differently from disk than over HTTP.
- `HashConfiguration` rejects a memory size below 4 KB per degree of parallelism with
  `ArgumentOutOfRangeException` when it is constructed. Argon2 cannot run with less, and such a configuration used to
  fail only when hashing, with an `AggregateException`.

### Fixed

- `Condition.ToProcessOutput` returns a failed output whenever the condition is not satisfied. A condition that
  failed with an empty or whitespace message converted into a successful `ProcessOutput`, because
  `ProcessOutput.AddErrors` drops such messages; such a failure is now reported as the new
  `Condition.BlankFailureMessage`.
- The XML documentation of `CharacterChecks` and `RegexCollection.HasNumberLowerAndUpperCharPattern` states that the
  composite pattern's `\d` accepts non-ASCII digits, instead of claiming the two agree on every input.

## [2.1.0] - 2026-08-24

### Added

- `AnsiColors.Reset`, the escape sequence that ends a colour.

### Changed

- `ArturRios.Output` updated from 3.1.0 to 3.2.0.

### Fixed

- The library links under "Documentation" in the README returned 404; they point at `/dotnet-util/docs/<page>/`.

## [2.0.0] - 2026-08-19

Several of the fixes in this release change results or signatures; what to change in calling code is described in
[Upgrading from 1.x to 2.0](#upgrading-from-1x-to-20).

### Added

- `HttpOutput.ContentHeaders`, `RawBody` and `IsSuccess`.
- `Hash.TextMatches` accepts the `HashConfiguration` the hash was produced with.
- `FileReaderAsync` methods take an optional `CancellationToken`.

### Changed

- **Breaking:** `Retry.MaxAttempts(n)` means *n total executions* instead of `n + 1`, and a reused `Retry` keeps its
  configuration.
- **Breaking:** `CustomRandom.NumberFromSystemRandom` treats `end` as inclusive.
- **Breaking:** `Characters.Special` gained the backtick, tilde and backslash.
- **Breaking:** `HttpOutput` and `HttpGateway` serialize with `System.Text.Json` instead of `Newtonsoft.Json`.
- **Breaking:** `Condition` reports duplicate failure messages once per failing condition, and `FailsWith` throws
  when no `True`/`False` precedes it.
- **Breaking:** `HttpOutput.StatusCode`, `Headers` and `Body` are read-only.
- **Breaking:** `HttpStatusCodes` groups are `ImmutableArray<int>` rather than `int[]`.
- **Breaking:** `PrimeGenerator<T>` is constrained to `IBinaryInteger<T>`.
- **Breaking:** `ConditionFailedException.Errors` is a property rather than a public field.
- **Breaking:** `Retry`, `JitteredWaiter`, `HashConfiguration`, `Hash` and `CustomRandom` reject out-of-range
  arguments, and `ReadAsDictionary` rejects duplicate header names.

### Removed

- **Breaking:** `JitteredWaiter.Wait()` and `HttpOutput.ReadContent()`, replaced by their async counterparts.
- The `Newtonsoft.Json` dependency.

### Fixed

- `PrimeUtils.IsPrimeNumber(long)` rejects negative values, and `IsPrimeNumber(BigInteger)` returns for large
  operands.
- `JitteredWaiter` waits are capped at `maxWaitMilliseconds` instead of overflowing.
- `NumberFromRng` accepts a range ending at `int.MaxValue`.
- `HttpOutput` no longer lets a body that does not match the expected shape escape to the caller as an exception.
- The Argon2 and HTTP resources that were leaking are disposed.

### Security

- `Hash.TextMatches` compares in constant time.

### Upgrading from 1.x to 2.0

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

## [1.6.0] - 2026-08-19

### Added

- `CharacterChecks` — `HasNumber`, `HasLowerChar`, `HasUpperChar` and `HasSpecialChar` over both `string` and
  `ReadOnlySpan<char>` — a vectorized alternative to the character class regexes.
- The `CharacterClasses` flags enum with `Classify` and `Missing`, which name the character classes a value lacks.
- `EmailAddress.TryNormalize` and `IsValid`, which lowercase the domain and punycode an internationalized one before
  applying `EmailPattern`.

### Changed

- `EmailPattern` is stricter: domain labels must start and end with an alphanumeric character, underscores are no
  longer allowed in the domain, the local part is ASCII only, and the top-level domain limit rises from 4 to 63
  characters. IPv4 hosts are accepted only in the bracketed form, with octets bounded to 0-255, so an unbracketed host
  such as `user@192.168.1.1` no longer matches.
- Every generated regex has a 100 ms match timeout.

### Fixed

- End-anchored patterns use `\z` instead of `$`, so a value with a trailing newline, such as `"user@host.com\n"`, no
  longer passes.

## [1.5.0] - 2026-07-30

### Changed

- `CustomRandom.Text` rejects requests it cannot satisfy: no character set enabled, or a length below the number of
  enabled sets.

### Fixed

- `CustomRandom.Text` builds its alphabet from the enabled character sets only; it used to pad the result from every
  set, ignoring the flags.

### Security

- `CustomRandom.Text` generates and shuffles with `RandomNumberGenerator` instead of `System.Random`.

## [1.4.2] - 2026-07-23

### Changed

- `ArturRios.Output` updated from 3.0.0 to 3.1.0.

## [1.4.1] - 2026-07-23

### Changed

- `ArturRios.Output` updated from 1.0.1 to 3.0.0.

## [1.4.0] - 2026-07-23

### Added

- `HttpStatusCodes` constants for 202, the 3xx redirection codes, 405, 409, 422, 429, 503 and 504, plus a
  `Redirection` group folded into `All`.

## [1.3.0] - 2026-07-21

### Added

- `CustomConsole.WriteCharLine` in the new `Console` namespace, to print a separator line of one repeated character.

## [1.2.0] - 2026-06-26

### Added

- `PrimeUtils` and `PrimeGenerator<T>` in the new `Math` namespace.

## [1.1.0] - 2025-12-15

### Added

- `HttpGateway`, `HttpOutput`, `HttpStatusCodes` and HTTP extension methods in the new `Http` namespace.

## [1.0.0] - 2025-12-05

### Added

- `AnsiColors` and `Characters` collections.
- Flow control: `Condition`, `Retry` and `JitteredWaiter`.
- Argon2id hashing with `Hash` and `HashConfiguration`.
- `FileReader` and `FileReaderAsync` file I/O helpers.
- `CustomRandom` random numbers and strings, configured with `RandomStringOptions`.
- `RegexCollection` and regex extension methods.

[Unreleased]: https://github.com/artur-rios/dotnet-util/compare/2.1.0...HEAD
[2.1.0]: https://github.com/artur-rios/dotnet-util/compare/v2.0.0...2.1.0
[2.0.0]: https://github.com/artur-rios/dotnet-util/compare/v1.6.0...v2.0.0
[1.6.0]: https://github.com/artur-rios/dotnet-util/compare/v1.5.0...v1.6.0
[1.5.0]: https://github.com/artur-rios/dotnet-util/compare/v1.4.2...v1.5.0
[1.4.2]: https://github.com/artur-rios/dotnet-util/compare/v1.4.1...v1.4.2
[1.4.1]: https://github.com/artur-rios/dotnet-util/compare/v1.4.0...v1.4.1
[1.4.0]: https://github.com/artur-rios/dotnet-util/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/artur-rios/dotnet-util/compare/v1.2.0...v1.3.0
[1.2.0]: https://github.com/artur-rios/dotnet-util/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/artur-rios/dotnet-util/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/artur-rios/dotnet-util/releases/tag/v1.0.0
