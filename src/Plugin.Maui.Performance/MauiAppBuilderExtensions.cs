using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Plugin.Maui.Performance;

/// <summary>
/// MAUI host registration for the performance profiler.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="IMauiPerformance"/> as a singleton and starts automatic
    /// startup, page, navigation, image, and render measurement.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.UseMauiPerformance(options =>
    /// {
    ///     options.AutoMeasureStartup = true;
    ///     options.AutoMeasurePages = true;
    /// });
    /// </code>
    /// </example>
    public static MauiAppBuilder UseMauiPerformance(this MauiAppBuilder builder, Action<MauiPerformanceOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new MauiPerformanceOptions();
        configure?.Invoke(options);

        builder.Services.AddMauiPerformance(options);
        builder.Services.AddTransient<IMauiInitializeService, MauiPerformanceInitializer>();

        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android =>
            {
                android.OnResume(_ => MauiPerformance.Current.Start());
            });
#elif IOS
            events.AddiOS(ios =>
            {
                ios.OnActivated(_ => MauiPerformance.Current.Start());
            });
#endif
        });

        return builder;
    }
}
