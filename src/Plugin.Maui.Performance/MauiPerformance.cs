namespace Plugin.Maui.Performance;

/// <summary>
/// Entry point for the performance plugin when dependency injection is not used.
/// </summary>
public static class MauiPerformance
{
    static IMauiPerformance? _current;

    /// <summary>
    /// Gets the shared <see cref="IMauiPerformance"/> instance.
    /// </summary>
    public static IMauiPerformance Current => _current ??= Create(new MauiPerformanceOptions());

    /// <summary>
    /// Starts a named timing. Dispose the returned value to record it:
    /// <code>
    /// using var trace = MauiPerformance.Trace("LoadCustomer");
    /// </code>
    /// </summary>
    public static PerformanceTrace Trace(string name, PerformanceCategory category = PerformanceCategory.Custom, IReadOnlyDictionary<string, string>? properties = null) =>
        Current.Trace(name, category, properties);

    /// <summary>
    /// Starts a database timing (default name <c>SQLite Query</c>).
    /// </summary>
    public static PerformanceTrace TraceDatabase(string name = "SQLite Query", IReadOnlyDictionary<string, string>? properties = null) =>
        Current.TraceDatabase(name, properties);

    /// <summary>
    /// Starts an API timing.
    /// </summary>
    public static PerformanceTrace TraceApi(string name, IReadOnlyDictionary<string, string>? properties = null) =>
        Current.TraceApi(name, properties);

    /// <summary>
    /// Starts an image-loading timing (default name <c>Image Loading</c>).
    /// </summary>
    public static PerformanceTrace TraceImage(string name = "Image Loading", IReadOnlyDictionary<string, string>? properties = null) =>
        Current.TraceImage(name, properties);

    /// <summary>
    /// Records an already-measured duration.
    /// </summary>
    public static void Record(string name, TimeSpan duration, PerformanceCategory category = PerformanceCategory.Custom, IReadOnlyDictionary<string, string>? properties = null) =>
        Current.Record(name, duration, category, properties);

    /// <summary>
    /// Times <paramref name="action"/> and records the result.
    /// </summary>
    public static void Measure(string name, Action action, PerformanceCategory category = PerformanceCategory.Custom) =>
        Current.Measure(name, action, category);

    /// <summary>
    /// Times <paramref name="action"/> and returns its result.
    /// </summary>
    public static T Measure<T>(string name, Func<T> action, PerformanceCategory category = PerformanceCategory.Custom) =>
        Current.Measure(name, action, category);

    /// <summary>
    /// Times an async action.
    /// </summary>
    public static Task MeasureAsync(string name, Func<Task> action, PerformanceCategory category = PerformanceCategory.Custom) =>
        Current.MeasureAsync(name, action, category);

    /// <summary>
    /// Times an async function and returns its result.
    /// </summary>
    public static Task<T> MeasureAsync<T>(string name, Func<Task<T>> action, PerformanceCategory category = PerformanceCategory.Custom) =>
        Current.MeasureAsync(name, action, category);

    /// <summary>
    /// Builds a report of the latest timing per name plus current memory.
    /// </summary>
    public static PerformanceReport GetReport() => Current.GetReport();

    /// <summary>
    /// Formats the report as a compact table.
    /// </summary>
    public static string FormatReport() => Current.FormatReport();

    /// <summary>
    /// Captures a memory sample now.
    /// </summary>
    public static MemorySnapshot CaptureMemory() => Current.CaptureMemory();

    /// <summary>
    /// Drops stored metrics.
    /// </summary>
    public static void Clear() => Current.Clear();

    /// <summary>
    /// Creates a profiler with platform memory probes and MAUI navigation hooks.
    /// </summary>
    public static IMauiPerformance Create(MauiPerformanceOptions? options = null) =>
        new MauiPerformanceImplementation(
            options ?? new MauiPerformanceOptions(),
            SystemClock.Instance,
            new PlatformPerformance());

    /// <summary>
    /// Replaces the shared instance. Intended for tests and custom implementations.
    /// </summary>
    public static void SetDefault(IMauiPerformance implementation) =>
        _current = implementation ?? throw new ArgumentNullException(nameof(implementation));

    internal static MauiPerformanceImplementation Create(
        MauiPerformanceOptions options,
        IClock clock,
        IPlatformPerformance platform,
        DateTimeOffset? processStartedAt = null) =>
        new(options, clock, platform, processStartedAt);
}
