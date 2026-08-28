namespace Plugin.Maui.Performance;

/// <summary>
/// Raised after a trace or automatic measurement is stored.
/// </summary>
public sealed class MetricRecordedEventArgs : EventArgs
{
    /// <summary>
    /// Creates event args for <paramref name="metric"/>.
    /// </summary>
    public MetricRecordedEventArgs(PerformanceMetric metric) =>
        Metric = metric ?? throw new ArgumentNullException(nameof(metric));

    /// <summary>The metric that was just recorded.</summary>
    public PerformanceMetric Metric { get; }
}
