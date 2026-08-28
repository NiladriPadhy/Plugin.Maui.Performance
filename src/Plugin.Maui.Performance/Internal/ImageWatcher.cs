#if ANDROID || IOS
using Microsoft.Maui;
using Microsoft.Maui.Controls;
#endif

namespace Plugin.Maui.Performance;

sealed class ImageWatcher : IDisposable
{
    readonly IMauiPerformanceListener _listener;
    readonly TimeSpan _timeout;
    int _generation;

    public ImageWatcher(IMauiPerformanceListener listener, TimeSpan timeout)
    {
        _listener = listener;
        _timeout = timeout <= TimeSpan.Zero ? MauiPerformanceDefaults.ImageLoadTimeout : timeout;
    }

    public void Watch(object? page, long startTimestamp)
    {
#if ANDROID || IOS
        if (page is not Page visual)
        {
            return;
        }

        var images = FindImages(visual).Where(static image => image.Source is not null).ToList();
        if (images.Count == 0)
        {
            return;
        }

        var generation = Interlocked.Increment(ref _generation);
        var remaining = images.Count;
        var completed = 0;

        void Finish()
        {
            if (Interlocked.Exchange(ref completed, 1) != 0 || generation != Volatile.Read(ref _generation))
            {
                return;
            }

            var duration = Stopwatch.GetElapsedTime(startTimestamp);
            _listener.OnImagesLoaded(duration);
        }

        foreach (var image in images)
        {
            if (image.IsLoaded && image.Width > 0)
            {
                if (Interlocked.Decrement(ref remaining) <= 0)
                {
                    Finish();
                }

                continue;
            }

            void OnLoaded(object? sender, EventArgs args)
            {
                image.Loaded -= OnLoaded;
                if (Interlocked.Decrement(ref remaining) <= 0)
                {
                    Finish();
                }
            }

            image.Loaded += OnLoaded;
        }

        var dispatcher = visual.Dispatcher;
        dispatcher.DispatchDelayed(_timeout, () =>
        {
            if (generation == Volatile.Read(ref _generation))
            {
                Finish();
            }
        });
#else
        _ = page;
        _ = startTimestamp;
#endif
    }

    public void Dispose() => Interlocked.Increment(ref _generation);

#if ANDROID || IOS
    static IEnumerable<Image> FindImages(IVisualTreeElement root)
    {
        if (root is Image image)
        {
            yield return image;
        }

        foreach (var child in root.GetVisualChildren())
        {
            foreach (var nested in FindImages(child))
            {
                yield return nested;
            }
        }
    }
#endif
}
