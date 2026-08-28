namespace Plugin.Maui.Performance;

/// <summary>
/// A running timing. Dispose it to store the metric:
/// <code>
/// using var trace = MauiPerformance.Trace("LoadCustomer");
/// </code>
/// </summary>
public sealed class PerformanceTrace : IDisposable
{
    readonly MauiPerformanceImplementation? _owner;
    readonly IClock _clock;
    readonly long _startTimestamp;
    readonly MemorySnapshot? _memoryAtStart;
    readonly IReadOnlyDictionary<string, string>? _properties;
    readonly bool _record;
    int _state;

    internal PerformanceTrace(
        MauiPerformanceImplementation? owner,
        IClock clock,
        string name,
        PerformanceCategory category,
        long startTimestamp,
        DateTimeOffset startedAt,
        MemorySnapshot? memoryAtStart,
        IReadOnlyDictionary<string, string>? properties,
        bool record)
    {
        _owner = owner;
        _clock = clock;
        Name = name;
        Category = category;
        _startTimestamp = startTimestamp;
        StartedAt = startedAt;
        _memoryAtStart = memoryAtStart;
        _properties = properties;
        _record = record;
    }

    /// <summary>Display name passed to <see cref="IMauiPerformance.Trace"/>.</summary>
    public string Name { get; }

    /// <summary>Category passed to <see cref="IMauiPerformance.Trace"/>.</summary>
    public PerformanceCategory Category { get; }

    /// <summary>When the trace started (UTC).</summary>
    public DateTimeOffset StartedAt { get; }

    /// <summary>Whether <see cref="Dispose"/> or <see cref="Cancel"/> has already run.</summary>
    public bool IsCompleted => Volatile.Read(ref _state) != 0;

    /// <summary>Live elapsed time, or the final duration after dispose.</summary>
    public TimeSpan Elapsed => _clock.GetElapsedTime(_startTimestamp);

    internal static PerformanceTrace Disabled(IClock clock, string name, PerformanceCategory category) =>
        new(null, clock, name, category, clock.GetTimestamp(), clock.UtcNow, null, null, record: false);

    /// <summary>
    /// Completes the trace and records a metric. Safe to call more than once.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _state, 1) != 0)
        {
            return;
        }

        if (!_record || _owner is null)
        {
            return;
        }

        _owner.Complete(
            Name,
            Category,
            _startTimestamp,
            StartedAt,
            _memoryAtStart,
            _properties);
    }

    /// <summary>
    /// Ends the trace without recording a metric.
    /// </summary>
    public void Cancel() => Interlocked.Exchange(ref _state, 1);
}
