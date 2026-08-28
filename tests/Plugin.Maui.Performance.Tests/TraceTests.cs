namespace Plugin.Maui.Performance.Tests;

public sealed class TraceTests
{
    [Fact]
    public void Using_trace_records_duration()
    {
        var (performance, clock, _) = Harness.Create();

        using (var trace = performance.Trace("LoadCustomer"))
        {
            Assert.Equal("LoadCustomer", trace.Name);
            Assert.False(trace.IsCompleted);
            clock.Advance(TimeSpan.FromMilliseconds(250));
        }

        var metric = Assert.Single(performance.GetMetrics());
        Assert.Equal("LoadCustomer", metric.Name);
        Assert.Equal(PerformanceCategory.Custom, metric.Category);
        Assert.Equal(TimeSpan.FromMilliseconds(250), metric.Duration);
        Assert.True(metric.IsCompleted);
        Assert.NotNull(metric.MemoryAtStart);
        Assert.NotNull(metric.MemoryAtEnd);
    }

    [Fact]
    public void Double_dispose_records_once()
    {
        var (performance, clock, _) = Harness.Create();
        var trace = performance.Trace("Once");
        clock.Advance(TimeSpan.FromMilliseconds(10));
        trace.Dispose();
        trace.Dispose();

        Assert.Single(performance.GetMetrics());
        Assert.True(trace.IsCompleted);
    }

    [Fact]
    public void Cancel_does_not_record()
    {
        var (performance, clock, _) = Harness.Create();
        var trace = performance.Trace("Abandoned");
        clock.Advance(TimeSpan.FromMilliseconds(40));
        trace.Cancel();
        trace.Dispose();

        Assert.Empty(performance.GetMetrics());
    }

    [Fact]
    public void Disabled_profiler_is_a_noop()
    {
        var (performance, _, _) = Harness.Create(options => options.Enabled = false);

        using (performance.Trace("LoadCustomer"))
        {
        }

        performance.Record("Home Page", TimeSpan.FromMilliseconds(420), PerformanceCategory.Page);
        Assert.False(performance.IsEnabled);
        Assert.Empty(performance.GetMetrics());
    }

    [Fact]
    public void Category_helpers_set_the_right_kind()
    {
        var (performance, clock, _) = Harness.Create();

        clock.Advance(TimeSpan.FromMilliseconds(18));
        performance.TraceDatabase().Dispose();
        clock.Advance(TimeSpan.FromMilliseconds(630));
        performance.TraceApi("Customer API").Dispose();
        clock.Advance(TimeSpan.FromMilliseconds(210));
        performance.TraceImage().Dispose();

        var metrics = performance.GetMetrics();
        Assert.Equal(PerformanceCategory.Database, metrics[0].Category);
        Assert.Equal("SQLite Query", metrics[0].Name);
        Assert.Equal(PerformanceCategory.Api, metrics[1].Category);
        Assert.Equal("Customer API", metrics[1].Name);
        Assert.Equal(PerformanceCategory.Image, metrics[2].Category);
        Assert.Equal("Image Loading", metrics[2].Name);
    }

    [Fact]
    public void Record_stores_an_explicit_duration()
    {
        var (performance, _, _) = Harness.Create();
        performance.Record("Home Page", TimeSpan.FromMilliseconds(420), PerformanceCategory.Page);

        var metric = Assert.Single(performance.GetMetrics());
        Assert.Equal(TimeSpan.FromMilliseconds(420), metric.Duration);
        Assert.Equal("420 ms", metric.FormatDuration());
    }

    [Fact]
    public void Ring_buffer_drops_oldest_metrics()
    {
        var (performance, _, _) = Harness.Create(options => options.MaxMetrics = 3);

        performance.Record("one", TimeSpan.FromMilliseconds(1));
        performance.Record("two", TimeSpan.FromMilliseconds(2));
        performance.Record("three", TimeSpan.FromMilliseconds(3));
        performance.Record("four", TimeSpan.FromMilliseconds(4));

        var names = performance.GetMetrics().Select(metric => metric.Name).ToArray();
        Assert.Equal(["two", "three", "four"], names);
    }

    [Fact]
    public void Clear_removes_metrics()
    {
        var (performance, _, _) = Harness.Create();
        performance.Record("Home Page", TimeSpan.FromMilliseconds(420), PerformanceCategory.Page);
        performance.Clear();
        Assert.Empty(performance.GetMetrics());
    }

    [Fact]
    public void Static_facade_forwards_to_current()
    {
        var (performance, clock, _) = Harness.Create();
        MauiPerformance.SetDefault(performance);

        clock.Advance(TimeSpan.FromMilliseconds(18));
        using (MauiPerformance.Trace("LoadCustomer"))
        {
        }

        Assert.Equal("LoadCustomer", Assert.Single(MauiPerformance.Current.GetMetrics()).Name);
        MauiPerformance.Clear();
        Assert.Empty(performance.GetMetrics());
    }

    [Fact]
    public void Metric_recorded_event_fires()
    {
        var (performance, _, _) = Harness.Create();
        PerformanceMetric? seen = null;
        performance.MetricRecorded += (_, args) => seen = args.Metric;

        performance.Record("Customer API", TimeSpan.FromMilliseconds(630), PerformanceCategory.Api);

        Assert.NotNull(seen);
        Assert.Equal("Customer API", seen!.Name);
    }

    [Fact]
    public void Trace_requires_a_name()
    {
        var (performance, _, _) = Harness.Create();
        Assert.Throws<ArgumentException>(() => performance.Trace("  "));
    }
}
