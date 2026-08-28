#if !ANDROID && !IOS
namespace Plugin.Maui.Performance;

sealed class PlatformPerformance : IPlatformPerformance
{
    public bool IsSupported => true;

    public MemorySnapshot CaptureMemory(DateTimeOffset capturedAt) =>
        MemoryProbe.Capture(capturedAt);

    public void ObserveNextFrame(Action<TimeSpan> onFrame)
    {
        ArgumentNullException.ThrowIfNull(onFrame);
        onFrame(TimeSpan.Zero);
    }
}
#endif
