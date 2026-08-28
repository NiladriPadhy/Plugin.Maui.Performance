namespace Plugin.Maui.Performance;

/// <summary>
/// One completed timing, plus optional memory around the work.
/// </summary>
public sealed class PerformanceMetric
{
    /// <summary>
    /// Creates a completed metric.
    /// </summary>
    public PerformanceMetric(
        string name,
        PerformanceCategory category,
        TimeSpan duration,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt,
        MemorySnapshot? memoryAtStart = null,
        MemorySnapshot? memoryAtEnd = null,
        IReadOnlyDictionary<string, string>? properties = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Category = category;
        Duration = duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        MemoryAtStart = memoryAtStart;
        MemoryAtEnd = memoryAtEnd;
        Properties = properties;
    }

    /// <summary>Display name (for example <c>Customer API</c>).</summary>
    public string Name { get; }

    /// <summary>What kind of work this was.</summary>
    public PerformanceCategory Category { get; }

    /// <summary>Elapsed time.</summary>
    public TimeSpan Duration { get; }

    /// <summary>UTC start.</summary>
    public DateTimeOffset StartedAt { get; }

    /// <summary>UTC end.</summary>
    public DateTimeOffset CompletedAt { get; }

    /// <summary>Memory when the trace started.</summary>
    public MemorySnapshot? MemoryAtStart { get; }

    /// <summary>Memory when the trace completed.</summary>
    public MemorySnapshot? MemoryAtEnd { get; }

    /// <summary>
    /// Working-set (or managed) delta in bytes. Positive means the process grew.
    /// </summary>
    public long? MemoryDeltaBytes
    {
        get
        {
            var start = MemoryAtStart?.WorkingSetBytes ?? MemoryAtStart?.ManagedBytes;
            var end = MemoryAtEnd?.WorkingSetBytes ?? MemoryAtEnd?.ManagedBytes;
            if (start is null || end is null)
            {
                return null;
            }

            return end.Value - start.Value;
        }
    }

    /// <summary>Optional extra fields (HTTP method, page name, …).</summary>
    public IReadOnlyDictionary<string, string>? Properties { get; }

    /// <summary>Always <c>true</c> for stored metrics.</summary>
    public bool IsCompleted => true;

    /// <summary>Formats <see cref="Duration"/> as <c>1.82 sec</c> or <c>420 ms</c>.</summary>
    public string FormatDuration() => PerformanceReportFormatter.FormatDuration(Duration);
}
