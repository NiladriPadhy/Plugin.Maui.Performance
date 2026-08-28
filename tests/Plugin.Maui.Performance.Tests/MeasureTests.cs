namespace Plugin.Maui.Performance.Tests;

public sealed class MeasureTests
{
    [Fact]
    public void Measure_runs_the_action_and_records()
    {
        var (performance, clock, _) = Harness.Create();
        var ran = false;

        clock.Advance(TimeSpan.FromMilliseconds(40));
        performance.Measure("work", () => ran = true);

        Assert.True(ran);
        Assert.Equal(TimeSpan.FromMilliseconds(40), Assert.Single(performance.GetMetrics()).Duration);
    }

    [Fact]
    public void Measure_returns_the_value()
    {
        var (performance, _, _) = Harness.Create();
        var value = performance.Measure("answer", () => 42, PerformanceCategory.Database);
        Assert.Equal(42, value);
        Assert.Equal(PerformanceCategory.Database, Assert.Single(performance.GetMetrics()).Category);
    }

    [Fact]
    public async Task MeasureAsync_awaits_and_records()
    {
        var (performance, clock, _) = Harness.Create();
        clock.Advance(TimeSpan.FromMilliseconds(12));

        await performance.MeasureAsync("async", () => Task.CompletedTask);

        Assert.Equal("async", Assert.Single(performance.GetMetrics()).Name);
    }

    [Fact]
    public async Task MeasureAsync_returns_the_value()
    {
        var (performance, _, _) = Harness.Create();
        var value = await performance.MeasureAsync("payload", () => Task.FromResult("ok"), PerformanceCategory.Api);
        Assert.Equal("ok", value);
        Assert.Equal(PerformanceCategory.Api, Assert.Single(performance.GetMetrics()).Category);
    }
}
