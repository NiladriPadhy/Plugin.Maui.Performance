#if ANDROID
#pragma warning disable CA1416, CA1422
using Android.App;
using Android.Content;
using Android.Views;
using MauiPlatform = Microsoft.Maui.ApplicationModel.Platform;

namespace Plugin.Maui.Performance;

sealed class PlatformPerformance : IPlatformPerformance
{
    public bool IsSupported => true;

    public MemorySnapshot CaptureMemory(DateTimeOffset capturedAt)
    {
        long? available = null;
        long? total = null;
        MemoryPressureKind? pressure = null;

        Try(() =>
        {
            if (MauiPlatform.AppContext.GetSystemService(Context.ActivityService) is not ActivityManager manager)
            {
                return;
            }

            var info = new ActivityManager.MemoryInfo();
            manager.GetMemoryInfo(info);
            available = info.AvailMem;
            total = info.TotalMem;

            if (info.LowMemory)
            {
                pressure = MemoryPressureKind.Critical;
            }
            else if (info.TotalMem > 0 && info.AvailMem <= info.Threshold)
            {
                pressure = MemoryPressureKind.Warning;
            }
            else
            {
                pressure = MemoryPressureKind.Normal;
            }
        });

        return MemoryProbe.Capture(capturedAt, available, total, pressure);
    }

    public void ObserveNextFrame(Action<TimeSpan> onFrame)
    {
        ArgumentNullException.ThrowIfNull(onFrame);

        try
        {
            var choreographer = Choreographer.Instance;
            if (choreographer is null)
            {
                onFrame(TimeSpan.Zero);
                return;
            }

            var start = Stopwatch.GetTimestamp();
            choreographer.PostFrameCallback(new NextFrameCallback(() =>
                onFrame(Stopwatch.GetElapsedTime(start))));
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

    sealed class NextFrameCallback(Action callback) : Java.Lang.Object, Choreographer.IFrameCallback
    {
        public void DoFrame(long frameTimeNanos)
        {
            try
            {
                callback();
            }
            catch
            {
                // Never crash the frame callback.
            }
        }
    }
}
#endif
