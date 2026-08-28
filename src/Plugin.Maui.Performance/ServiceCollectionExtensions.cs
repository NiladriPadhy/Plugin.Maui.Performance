namespace Plugin.Maui.Performance;

/// <summary>
/// Registers performance services without MAUI lifecycle hooks.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IMauiPerformance"/> using the supplied options instance.
    /// </summary>
    public static IServiceCollection AddMauiPerformance(this IServiceCollection services, MauiPerformanceOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);
        services.TryAddSingleton<IMauiPerformance>(sp =>
        {
            var performance = MauiPerformance.Create(options);
            MauiPerformance.SetDefault(performance);
            return performance;
        });

        return services;
    }

    /// <summary>
    /// Adds <see cref="IMauiPerformance"/> and applies <paramref name="configure"/> to a new options instance.
    /// </summary>
    public static IServiceCollection AddMauiPerformance(this IServiceCollection services, Action<MauiPerformanceOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new MauiPerformanceOptions();
        configure?.Invoke(options);
        return services.AddMauiPerformance(options);
    }
}
