namespace Plugin.Maui.Performance;

/// <summary>
/// Point-in-time memory reading for a trace or report.
/// </summary>
public sealed class MemorySnapshot
{
    /// <summary>When the sample was taken (UTC).</summary>
    public DateTimeOffset CapturedAt { get; init; }

    /// <summary>Bytes reported by <see cref="GC.GetTotalMemory"/>.</summary>
    public long? ManagedBytes { get; init; }

    /// <summary>Process working set, when the host exposes it.</summary>
    public long? WorkingSetBytes { get; init; }

    /// <summary>Estimated free RAM for the process / device.</summary>
    public long? AvailableBytes { get; init; }

    /// <summary>Total physical RAM, when known.</summary>
    public long? TotalBytes { get; init; }

    /// <summary>Coarse pressure from the OS or free-RAM ratio.</summary>
    public MemoryPressureKind Pressure { get; init; }

    /// <summary>
    /// Formats working set (or managed heap) as a short <c>184 MB</c> label.
    /// </summary>
    public string FormatUsed()
    {
        var bytes = WorkingSetBytes ?? ManagedBytes;
        return bytes is null ? "n/a" : FormatBytes(bytes.Value);
    }

    /// <summary>
    /// Formats a byte count as MB with no fractional digits below 10 MB, one digit above.
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        var mb = bytes / (1024d * 1024d);
        return mb >= 10 ? $"{mb:0} MB" : $"{mb:0.0} MB";
    }
}
