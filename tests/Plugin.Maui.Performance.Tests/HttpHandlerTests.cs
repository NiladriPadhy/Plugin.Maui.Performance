namespace Plugin.Maui.Performance.Tests;

public sealed class HttpHandlerTests
{
    [Fact]
    public async Task Records_path_segment_as_api_name()
    {
        var (performance, clock, _) = Harness.Create();
        clock.Advance(TimeSpan.FromMilliseconds(630));

        using var handler = new PerformanceDelegatingHandler(new StubHandler(), performance);
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("https://api.shop/customer?token=secret");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var metric = Assert.Single(performance.GetMetrics());
        Assert.Equal("Customer API", metric.Name);
        Assert.Equal(PerformanceCategory.Api, metric.Category);
        Assert.Equal(TimeSpan.FromMilliseconds(630), metric.Duration);
        Assert.Equal("https://api.shop/customer", metric.Properties?["url"]);
        Assert.Equal("GET", metric.Properties?["method"]);
    }

    [Fact]
    public async Task Custom_name_overrides_the_path()
    {
        var (performance, _, _) = Harness.Create();
        using var handler = new PerformanceDelegatingHandler(new StubHandler(), performance);
        using var client = new HttpClient(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.shop/v1/users");
        request.Options.Set(PerformanceHttp.NameKey, "Customer API");
        await client.SendAsync(request);

        Assert.Equal("Customer API", Assert.Single(performance.GetMetrics()).Name);
    }

    sealed class StubHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
