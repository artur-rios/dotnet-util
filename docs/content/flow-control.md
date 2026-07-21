+++
title          = "Flow Control"
show_nav       = true
nav_back_label = "Console"
nav_back_url   = "/dotnet-util/console"
nav_next_label = "Hashing"
nav_next_url   = "/dotnet-util/hashing"
+++

## Features

- `Condition`: fluent condition aggregator; collects failure messages and can throw `ConditionFailedException` or convert to a process output.
- `ConditionFailedException`: exception carrying all collected error messages as `string[]`.
- `Retry`: simple retry with configurable max attempts and fixed delay; supports `Action` and `Func<T>`.
- `JitteredWaiter`: exponential backoff with jitter; exposes `CanRetry` and throws `MaxRetriesReachedException` when retries are exhausted.
- `MaxRetriesReachedException`: exception thrown when `JitteredWaiter` exceeds its retry limit.

## Class Diagram

```mermaid
classDiagram
    namespace FlowControl {
        class Condition {
            -HashSet~string~ _failedConditions
            -bool _expression
            +static Condition Create
            +string[] FailedConditions
            +bool IsSatisfied
            +Condition True(bool expression)
            +Condition False(bool expression)
            +Condition FailsWith(string error)
            +void ThrowIfNotSatisfied()
            +ProcessOutput ToProcessOutput()
        }
        class ConditionFailedException {
            +string[] Errors
        }
        class Retry {
            -int _delayMilliseconds
            -int _maxAttempts
            +static Retry New
            +Retry MaxAttempts(int maxAttempts)
            +Retry DelayMilliseconds(int delayMilliseconds)
            +void Execute(Action action)
            +T Execute~T~(Func~T~ func)
        }
    }
    namespace FlowControl_Waiter {
        class JitteredWaiter {
            -const int FixedWaitDelay
            +int MaxRetryCount
            -int Count
            +bool CanRetry
            +Task Wait()
        }
        class MaxRetriesReachedException
    }
    Condition ..> ConditionFailedException : throws
    JitteredWaiter ..> MaxRetriesReachedException : throws
```

## Usage

### Condition

Chain boolean assertions — all failures are collected before throwing:

```csharp
using ArturRios.Util.FlowControl;

Condition.Create
    .True(user is not null).FailsWith("User is required")
    .True(emailValid).FailsWith("Invalid email format")
    .False(string.IsNullOrEmpty(username)).FailsWith("Username cannot be empty")
    .ThrowIfNotSatisfied();
```

Convert to a process output instead of throwing:

```csharp
var result = Condition.Create
    .True(age >= 18).FailsWith("Must be 18 or older")
    .ToProcessOutput();

if (!result.Success)
    Console.WriteLine(string.Join(", ", result.Errors));
```

### Retry

Retry a void action:

```csharp
using ArturRios.Util.FlowControl;

Retry.New
    .MaxAttempts(3)
    .DelayMilliseconds(200)
    .Execute(() => SendEmail(recipient));
```

Retry a function that returns a value:

```csharp
var data = Retry.New
    .MaxAttempts(5)
    .DelayMilliseconds(500)
    .Execute(() => FetchDataFromApi());
```

### JitteredWaiter

Use in a polling or retry loop with exponential backoff:

```csharp
using ArturRios.Util.FlowControl.Waiter;

var waiter = new JitteredWaiter(maxRetryCount: 5);

while (waiter.CanRetry)
{
    try
    {
        await TryOperationAsync();
        break;
    }
    catch (TransientException)
    {
        await waiter.Wait();
    }
}
```
