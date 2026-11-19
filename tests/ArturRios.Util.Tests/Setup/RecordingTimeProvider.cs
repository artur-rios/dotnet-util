using System.Collections.Concurrent;
using Microsoft.Extensions.Time.Testing;

namespace ArturRios.Util.Tests.Setup;

/// <summary>
/// A <see cref="FakeTimeProvider"/> that records the due time of every timer created on it, so a test can assert the
/// exact delays a waiter asked for, and can wait for a delay to start, from another thread, before advancing past it.
/// </summary>
/// <remarks>
/// Every wait the library times by a <see cref="TimeProvider"/> is a timer created on it, so the due times are the
/// waits, in the order they began.
/// </remarks>
public sealed class RecordingTimeProvider : FakeTimeProvider
{
    // Only a guard against a test hanging when no wait starts: a passing test never waits this long.
    private static readonly TimeSpan NoTimerTimeout = TimeSpan.FromSeconds(10);

    private readonly ConcurrentQueue<TimeSpan> _dueTimes = new();
    private readonly ConcurrentQueue<TimeSpan> _notTakenYet = new();
    private readonly SemaphoreSlim _created = new(0);

    /// <summary>The due time of every timer created so far, in the order they were created.</summary>
    public IReadOnlyList<TimeSpan> DueTimes => [.. _dueTimes];

    /// <inheritdoc />
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = base.CreateTimer(callback, state, dueTime, period);

        _dueTimes.Enqueue(dueTime);
        _notTakenYet.Enqueue(dueTime);
        _created.Release();

        return timer;
    }

    /// <summary>Waits for the next timer to be created, and returns its due time without advancing the clock.</summary>
    /// <exception cref="TimeoutException">No timer was created within ten seconds of real time.</exception>
    public async Task<TimeSpan> NextTimerAsync(CancellationToken cancellationToken)
    {
        if (!await _created.WaitAsync(NoTimerTimeout, cancellationToken))
        {
            throw new TimeoutException("No wait started on the clock.");
        }

        _notTakenYet.TryDequeue(out var dueTime);

        return dueTime;
    }

    /// <summary>Waits for the next timer to be created and advances the clock to its due time, ending that wait.</summary>
    /// <returns>The due time the clock was advanced by.</returns>
    public async Task<TimeSpan> AdvanceThroughNextTimerAsync(CancellationToken cancellationToken)
    {
        var dueTime = await NextTimerAsync(cancellationToken);

        Advance(dueTime);

        return dueTime;
    }
}
