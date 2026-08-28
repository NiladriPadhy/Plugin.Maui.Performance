using Microsoft.Maui.Hosting;

namespace Plugin.Maui.Performance;

sealed class MauiPerformanceInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        var performance = services.GetService<IMauiPerformance>() ?? MauiPerformance.Current;
        MauiPerformance.SetDefault(performance);
        performance.Start();
    }
}
