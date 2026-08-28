namespace Plugin.Maui.Performance;

/// <summary>
/// Default values for <see cref="MauiPerformanceOptions"/>.
/// </summary>
public static class MauiPerformanceDefaults
{
    /// <summary>How many completed metrics to keep in memory.</summary>
    public const int MaxMetrics = 200;

    /// <summary>How long to wait for images on a page before closing the load trace.</summary>
    public static readonly TimeSpan ImageLoadTimeout = TimeSpan.FromSeconds(8);

    /// <summary>
    /// If process start is older than this when <see cref="IMauiPerformance.Start"/> runs,
    /// startup is measured from <c>Start</c> instead (hot reload / tests).
    /// </summary>
    public static readonly TimeSpan MaxStartupLookback = TimeSpan.FromSeconds(60);
}
