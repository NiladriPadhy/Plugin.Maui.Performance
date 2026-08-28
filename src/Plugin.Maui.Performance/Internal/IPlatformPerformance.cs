namespace Plugin.Maui.Performance;

interface IPlatformPerformance
{
    bool IsSupported { get; }

    MemorySnapshot CaptureMemory(DateTimeOffset capturedAt);

    void ObserveNextFrame(Action<TimeSpan> onFrame);
}

interface IMauiPerformanceListener
{
    void OnPageAppearing(string pageName, object? page);

    void OnPageDisappearing(string pageName, object? page);

    void OnPageLoaded(string pageName, object? page);

    void OnImagesLoaded(TimeSpan duration);
}
