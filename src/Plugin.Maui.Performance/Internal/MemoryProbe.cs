namespace Plugin.Maui.Performance;

static class MemoryProbe
{
    public static MemorySnapshot Capture(
        DateTimeOffset capturedAt,
        long? availableBytes = null,
        long? totalBytes = null,
        MemoryPressureKind? pressure = null)
    {
        long? managed = null;
        long? workingSet = null;

        try
        {
            managed = GC.GetTotalMemory(forceFullCollection: false);
        }
        catch
        {
            // Never fail the host app for a sample.
        }

        try
        {
            using var process = Process.GetCurrentProcess();
            workingSet = process.WorkingSet64;
        }
        catch
        {
            // Some sandboxes hide process stats.
        }

        var resolvedPressure = pressure ?? InferPressure(availableBytes, totalBytes);

        return new MemorySnapshot
        {
            CapturedAt = capturedAt,
            ManagedBytes = managed,
            WorkingSetBytes = workingSet,
            AvailableBytes = availableBytes,
            TotalBytes = totalBytes,
            Pressure = resolvedPressure
        };
    }

    public static MemoryPressureKind InferPressure(long? availableBytes, long? totalBytes)
    {
        if (availableBytes is not > 0 || totalBytes is not > 0)
        {
            return MemoryPressureKind.Unknown;
        }

        var usedPercent = (totalBytes.Value - availableBytes.Value) * 100d / totalBytes.Value;
        return usedPercent >= 92
            ? MemoryPressureKind.Critical
            : usedPercent >= 80
                ? MemoryPressureKind.Warning
                : MemoryPressureKind.Normal;
    }
}
