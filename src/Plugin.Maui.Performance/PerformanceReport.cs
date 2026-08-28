namespace Plugin.Maui.Performance;

/// <summary>
/// Latest timings per name, the full ring buffer, and a memory sample.
/// </summary>
public sealed class PerformanceReport
{
    /// <summary>
    /// Creates a report.
    /// </summary>
    public PerformanceReport(
        IReadOnlyList<PerformanceMetric> metrics,
        IReadOnlyList<PerformanceMetric> allMetrics,
        MemorySnapshot memory,
        DateTimeOffset generatedAt)
    {
        Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        AllMetrics = allMetrics ?? throw new ArgumentNullException(nameof(allMetrics));
        Memory = memory ?? throw new ArgumentNullException(nameof(memory));
        GeneratedAt = generatedAt;
    }

    /// <summary>
    /// Latest completed metric per name, ordered for display
    /// (startup, page, navigation, API, database, image, render, then custom).
    /// </summary>
    public IReadOnlyList<PerformanceMetric> Metrics { get; }

    /// <summary>Every retained metric, oldest first.</summary>
    public IReadOnlyList<PerformanceMetric> AllMetrics { get; }

    /// <summary>Memory at report time.</summary>
    public MemorySnapshot Memory { get; }

    /// <summary>When this report was built (UTC).</summary>
    public DateTimeOffset GeneratedAt { get; }

    /// <summary>
    /// Compact table plus a memory line:
    /// <code>
    /// App Startup       1.82 sec
    /// Home Page         420 ms
    /// Customer API      630 ms
    /// </code>
    /// </summary>
    public string Format() => PerformanceReportFormatter.Format(this);
}
