#if IOS
using System.Runtime.InteropServices;
using CoreAnimation;
using Foundation;

namespace Plugin.Maui.Performance;

sealed class PlatformPerformance : IPlatformPerformance
{
    public bool IsSupported => true;

    public MemorySnapshot CaptureMemory(DateTimeOffset capturedAt)
    {
        long? available = null;
        long? total = null;

        Try(() =>
        {
            total = (long)NSProcessInfo.ProcessInfo.PhysicalMemory;
            var remaining = (long)OsProcAvailableMemory();
            if (remaining >= 0)
            {
                available = remaining;
            }
        });

        return MemoryProbe.Capture(capturedAt, available, total);
    }

    public void ObserveNextFrame(Action<TimeSpan> onFrame)
    {
        ArgumentNullException.ThrowIfNull(onFrame);

        try
        {
            var start = Stopwatch.GetTimestamp();
            CADisplayLink? link = null;
            link = CADisplayLink.Create(() =>
            {
                try
                {
                    link?.Invalidate();
                    onFrame(Stopwatch.GetElapsedTime(start));
                }
                catch
                {
                    // Never crash the display link.
                }
            });
            link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
        }
        catch
        {
            onFrame(TimeSpan.Zero);
        }
    }

    static void Try(Action collect)
    {
        try
        {
            collect();
        }
        catch
        {
            // Platform probes must never throw into the host app.
        }
    }

    [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "os_proc_available_memory")]
    static extern nuint OsProcAvailableMemory();
}
#endif
