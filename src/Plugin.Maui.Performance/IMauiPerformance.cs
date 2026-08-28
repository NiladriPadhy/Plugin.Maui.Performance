namespace Plugin.Maui.Performance;

/// <summary>
/// Lightweight profiler: named traces, automatic MAUI hooks, and a compact timing report.
/// </summary>
public interface IMauiPerformance
{
    /// <summary>
    /// Gets a value indicating whether this target can collect timings.
    /// Always <c>true</c> for Android, iOS, and the shared <c>net10.0</c> surface.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Gets a value indicating whether automatic hooks have been started.
    /// </summary>
    bool IsStarted { get; }

    /// <summary>
    /// Gets a value indicating whether traces are recorded.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Raised after a metric is stored.
    /// </summary>
    event EventHandler<MetricRecordedEventArgs>? MetricRecorded;

    /// <summary>
    /// Turns on automatic page, navigation, image, and render hooks.
    /// Safe to call more than once.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops automatic hooks. Existing metrics are kept.
    /// </summary>
    void Stop();

    /// <summary>
    /// Starts a named timing. Dispose the returned value to record it:
    /// <code>
    /// using var trace = MauiPerformance.Trace("LoadCustomer");
    /// </code>
    /// </summary>
    PerformanceTrace Trace(string name, PerformanceCategory category = PerformanceCategory.Custom, IReadOnlyDictionary<string, string>? properties = null);

    /// <summary>
    /// Starts a database timing (default name <c>SQLite Query</c>).
    /// </summary>
    PerformanceTrace TraceDatabase(string name = "SQLite Query", IReadOnlyDictionary<string, string>? properties = null);

    /// <summary>
    /// Starts an API timing.
    /// </summary>
    PerformanceTrace TraceApi(string name, IReadOnlyDictionary<string, string>? properties = null);

    /// <summary>
    /// Starts an image-loading timing (default name <c>Image Loading</c>).
    /// </summary>
    PerformanceTrace TraceImage(string name = "Image Loading", IReadOnlyDictionary<string, string>? properties = null);

    /// <summary>
    /// Records an already-measured duration.
    /// </summary>
    void Record(string name, TimeSpan duration, PerformanceCategory category = PerformanceCategory.Custom, IReadOnlyDictionary<string, string>? properties = null);

    /// <summary>
    /// Times <paramref name="action"/> and records the result.
    /// </summary>
    void Measure(string name, Action action, PerformanceCategory category = PerformanceCategory.Custom);

    /// <summary>
    /// Times <paramref name="action"/> and returns its result.
    /// </summary>
    T Measure<T>(string name, Func<T> action, PerformanceCategory category = PerformanceCategory.Custom);

    /// <summary>
    /// Times an async action.
    /// </summary>
    Task MeasureAsync(string name, Func<Task> action, PerformanceCategory category = PerformanceCategory.Custom);

    /// <summary>
    /// Times an async function and returns its result.
    /// </summary>
    Task<T> MeasureAsync<T>(string name, Func<Task<T>> action, PerformanceCategory category = PerformanceCategory.Custom);

    /// <summary>
    /// Returns retained metrics, oldest first.
    /// </summary>
    IReadOnlyList<PerformanceMetric> GetMetrics();

    /// <summary>
    /// Builds a report of the latest timing per name plus current memory.
    /// </summary>
    PerformanceReport GetReport();

    /// <summary>
    /// Formats the report as a compact table.
    /// </summary>
    string FormatReport();

    /// <summary>
    /// Captures a memory sample now.
    /// </summary>
    MemorySnapshot CaptureMemory();

    /// <summary>
    /// Drops stored metrics. Does not stop automatic hooks.
    /// </summary>
    void Clear();
}
