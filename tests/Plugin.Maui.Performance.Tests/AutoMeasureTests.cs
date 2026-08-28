namespace Plugin.Maui.Performance.Tests;

public sealed class AutoMeasureTests
{
    [Fact]
    public void First_page_records_app_startup()
    {
        var started = new DateTimeOffset(2026, 8, 28, 14, 0, 0, TimeSpan.Zero);
        var (performance, clock, _) = Harness.Create(
            options =>
            {
                options.AutoMeasureStartup = true;
                options.AutoMeasurePages = false;
                options.AutoMeasureNavigation = false;
                options.AutoMeasureImages = false;
                options.AutoMeasureRendering = false;
            },
            started);

        clock.Advance(TimeSpan.FromSeconds(1.82));
        performance.OnPageAppearing("Home Page", page: null);

        var metric = Assert.Single(performance.GetMetrics());
        Assert.Equal("App Startup", metric.Name);
        Assert.Equal(PerformanceCategory.Startup, metric.Category);
        Assert.Equal(TimeSpan.FromSeconds(1.82), metric.Duration);
    }

    [Fact]
    public void Startup_is_recorded_only_once()
    {
        var (performance, clock, _) = Harness.Create(options =>
        {
            options.AutoMeasureStartup = true;
            options.AutoMeasurePages = false;
            options.AutoMeasureNavigation = false;
            options.AutoMeasureImages = false;
            options.AutoMeasureRendering = false;
        });

        clock.Advance(TimeSpan.FromMilliseconds(100));
        performance.OnPageAppearing("Home Page", page: null);
        performance.OnPageAppearing("Customer Page", page: null);

        Assert.Single(performance.GetMetrics());
    }

    [Fact]
    public void Page_ready_records_page_metric()
    {
        var (performance, clock, _) = Harness.Create(options =>
        {
            options.AutoMeasureStartup = false;
            options.AutoMeasurePages = true;
            options.AutoMeasureNavigation = false;
            options.AutoMeasureImages = false;
            options.AutoMeasureRendering = false;
        });

        performance.OnPageAppearing("Home Page", page: null);
        clock.Advance(TimeSpan.FromMilliseconds(420));
        performance.OnPageLoaded("Home Page", page: null);

        var metric = Assert.Single(performance.GetMetrics());
        Assert.Equal("Home Page", metric.Name);
        Assert.Equal(PerformanceCategory.Page, metric.Category);
        Assert.Equal(TimeSpan.FromMilliseconds(420), metric.Duration);
    }

    [Fact]
    public void Navigation_is_measured_between_pages()
    {
        var (performance, clock, _) = Harness.Create(options =>
        {
            options.AutoMeasureStartup = false;
            options.AutoMeasurePages = false;
            options.AutoMeasureNavigation = true;
            options.AutoMeasureImages = false;
            options.AutoMeasureRendering = false;
        });

        performance.OnPageDisappearing("Home Page", page: null);
        clock.Advance(TimeSpan.FromMilliseconds(90));
        performance.OnPageAppearing("Customer Page", page: null);

        var metric = Assert.Single(performance.GetMetrics());
        Assert.Equal("Home Page → Customer Page", metric.Name);
        Assert.Equal(PerformanceCategory.Navigation, metric.Category);
        Assert.Equal(TimeSpan.FromMilliseconds(90), metric.Duration);
    }

    [Fact]
    public void Images_and_render_are_recorded()
    {
        var (performance, clock, _) = Harness.Create(options =>
        {
            options.AutoMeasureStartup = false;
            options.AutoMeasurePages = false;
            options.AutoMeasureNavigation = false;
            options.AutoMeasureImages = true;
            options.AutoMeasureRendering = true;
        });

        performance.OnImagesLoaded(TimeSpan.FromMilliseconds(210));
        clock.Advance(TimeSpan.FromMilliseconds(16));
        performance.OnPageLoaded("Home Page", page: null);

        var metrics = performance.GetMetrics();
        Assert.Contains(metrics, metric => metric.Name == "Image Loading" && metric.Duration == TimeSpan.FromMilliseconds(210));
        Assert.Contains(metrics, metric => metric.Name == "UI Render" && metric.Category == PerformanceCategory.Render);
    }
}
