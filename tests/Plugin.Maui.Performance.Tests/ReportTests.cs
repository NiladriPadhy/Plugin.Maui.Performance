namespace Plugin.Maui.Performance.Tests;

public sealed class ReportTests
{
    [Fact]
    public void Format_matches_the_product_table()
    {
        var (performance, _, _) = Harness.Create();
        performance.Record("App Startup", TimeSpan.FromSeconds(1.82), PerformanceCategory.Startup);
        performance.Record("Home Page", TimeSpan.FromMilliseconds(420), PerformanceCategory.Page);
        performance.Record("Customer API", TimeSpan.FromMilliseconds(630), PerformanceCategory.Api);
        performance.Record("SQLite Query", TimeSpan.FromMilliseconds(18), PerformanceCategory.Database);
        performance.Record("Image Loading", TimeSpan.FromMilliseconds(210), PerformanceCategory.Image);

        var table = PerformanceReportFormatter.Format(performance.GetReport().Metrics);
        var expected =
            "App Startup       1.82 sec" + Environment.NewLine +
            "Home Page           420 ms" + Environment.NewLine +
            "Customer API        630 ms" + Environment.NewLine +
            "SQLite Query         18 ms" + Environment.NewLine +
            "Image Loading       210 ms";

        Assert.Equal(expected, table);
    }

    [Fact]
    public void Report_keeps_latest_metric_per_name()
    {
        var (performance, _, _) = Harness.Create();
        performance.Record("Customer API", TimeSpan.FromMilliseconds(900), PerformanceCategory.Api);
        performance.Record("Customer API", TimeSpan.FromMilliseconds(630), PerformanceCategory.Api);

        var report = performance.GetReport();
        Assert.Equal(2, report.AllMetrics.Count);
        var latest = Assert.Single(report.Metrics);
        Assert.Equal(TimeSpan.FromMilliseconds(630), latest.Duration);
    }

    [Fact]
    public void Report_orders_by_category()
    {
        var (performance, _, _) = Harness.Create();
        performance.Record("SQLite Query", TimeSpan.FromMilliseconds(18), PerformanceCategory.Database);
        performance.Record("App Startup", TimeSpan.FromSeconds(1.82), PerformanceCategory.Startup);
        performance.Record("Customer API", TimeSpan.FromMilliseconds(630), PerformanceCategory.Api);
        performance.Record("Home Page", TimeSpan.FromMilliseconds(420), PerformanceCategory.Page);

        var names = performance.GetReport().Metrics.Select(metric => metric.Name).ToArray();
        Assert.Equal(["App Startup", "Home Page", "Customer API", "SQLite Query"], names);
    }

    [Fact]
    public void FormatDuration_uses_seconds_at_one_second()
    {
        Assert.Equal("1.00 sec", PerformanceReportFormatter.FormatDuration(TimeSpan.FromSeconds(1)));
        Assert.Equal("999 ms", PerformanceReportFormatter.FormatDuration(TimeSpan.FromMilliseconds(999)));
        Assert.Equal("0 ms", PerformanceReportFormatter.FormatDuration(TimeSpan.FromMilliseconds(-3)));
    }

    [Fact]
    public void FormatReport_appends_memory()
    {
        var (performance, _, _) = Harness.Create();
        performance.Record("Home Page", TimeSpan.FromMilliseconds(420), PerformanceCategory.Page);

        var text = performance.FormatReport();
        Assert.Contains("Home Page", text, StringComparison.Ordinal);
        Assert.Contains("420 ms", text, StringComparison.Ordinal);
        Assert.Contains("Memory", text, StringComparison.Ordinal);
        Assert.Contains("184 MB", text, StringComparison.Ordinal);
        Assert.Contains("512 MB avail", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Empty_metrics_still_can_show_memory()
    {
        var (performance, _, _) = Harness.Create();
        var text = performance.FormatReport();
        Assert.Contains("Memory", text, StringComparison.Ordinal);
    }
}
