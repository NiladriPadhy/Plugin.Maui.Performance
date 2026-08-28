namespace Plugin.Maui.Performance;

/// <summary>
/// <see cref="HttpMessageHandler"/> that records API latency.
/// Set <see cref="PerformanceHttp.NameKey"/> on the request to override the metric name
/// (for example <c>Customer API</c>).
/// </summary>
public sealed class PerformanceDelegatingHandler : DelegatingHandler
{
    readonly IMauiPerformance _performance;

    /// <summary>
    /// Creates a handler that writes to <see cref="MauiPerformance.Current"/>.
    /// </summary>
    public PerformanceDelegatingHandler()
        : this(MauiPerformance.Current)
    {
    }

    /// <summary>
    /// Creates a handler that writes to <paramref name="performance"/>.
    /// </summary>
    public PerformanceDelegatingHandler(IMauiPerformance performance)
    {
        _performance = performance ?? throw new ArgumentNullException(nameof(performance));
    }

    /// <summary>
    /// Creates a handler around an inner handler.
    /// </summary>
    public PerformanceDelegatingHandler(HttpMessageHandler innerHandler, IMauiPerformance? performance = null)
        : base(innerHandler)
    {
        _performance = performance ?? MauiPerformance.Current;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = ResolveName(request);
        var properties = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["method"] = request.Method.Method,
            ["url"] = Sanitize(request.RequestUri)
        };

        using var trace = _performance.Trace(name, PerformanceCategory.Api, properties);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    static string ResolveName(HttpRequestMessage request)
    {
        if (request.Options.TryGetValue(PerformanceHttp.NameKey, out var named) && !string.IsNullOrWhiteSpace(named))
        {
            return named.Trim();
        }

        var uri = request.RequestUri;
        if (uri is null)
        {
            return "API";
        }

        var segment = uri.AbsolutePath
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .LastOrDefault();

        if (string.IsNullOrEmpty(segment))
        {
            return string.IsNullOrEmpty(uri.Host) ? "API" : uri.Host;
        }

        return ToTitle(segment) + " API";
    }

    static string Sanitize(Uri? uri)
    {
        if (uri is null)
        {
            return string.Empty;
        }

        return string.IsNullOrEmpty(uri.Query)
            ? uri.ToString()
            : uri.GetLeftPart(UriPartial.Path);
    }

    static string ToTitle(string value)
    {
        if (value.Length == 0)
        {
            return value;
        }

        var buffer = value.ToCharArray();
        buffer[0] = char.ToUpperInvariant(buffer[0]);
        for (var i = 1; i < buffer.Length; i++)
        {
            buffer[i] = char.ToLowerInvariant(buffer[i]);
        }

        return new string(buffer);
    }
}

/// <summary>
/// Keys used by <see cref="PerformanceDelegatingHandler"/> to name a request.
/// </summary>
public static class PerformanceHttp
{
    /// <summary>
    /// Set this on <see cref="HttpRequestMessage.Options"/> to override the metric name
    /// (for example <c>Customer API</c>).
    /// </summary>
    public static readonly HttpRequestOptionsKey<string> NameKey = new("Plugin.Maui.Performance.Name");
}
