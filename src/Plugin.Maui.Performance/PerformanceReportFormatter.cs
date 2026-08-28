namespace Plugin.Maui.Performance;

/// <summary>
/// Formats metrics as a left-aligned name column and a right-aligned duration column.
/// </summary>
public static class PerformanceReportFormatter
{
    /// <summary>
    /// Builds the compact report, optionally appending a memory line.
    /// </summary>
    public static string Format(PerformanceReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var table = Format(report.Metrics);
        var memory = FormatMemory(report.Memory);
        if (string.IsNullOrEmpty(table))
        {
            return memory;
        }

        return string.IsNullOrEmpty(memory)
            ? table
            : table + Environment.NewLine + Environment.NewLine + memory;
    }

    /// <summary>
    /// Formats rows as <c>Name               420 ms</c>.
    /// </summary>
    public static string Format(IEnumerable<PerformanceMetric> metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var rows = metrics as IReadOnlyList<PerformanceMetric> ?? metrics.ToList();
        if (rows.Count == 0)
        {
            return string.Empty;
        }

        var names = rows.Select(static row => row.Name).ToList();
        var durations = rows.Select(static row => FormatDuration(row.Duration)).ToList();
        var nameWidth = Math.Max(16, names.Max(static name => name.Length));
        var durationWidth = durations.Max(static value => value.Length);

        var builder = new StringBuilder();
        for (var i = 0; i < rows.Count; i++)
        {
            if (i > 0)
            {
                builder.AppendLine();
            }

            builder.Append(names[i].PadRight(nameWidth));
            builder.Append("  ");
            builder.Append(durations[i].PadLeft(durationWidth));
        }

        return builder.ToString();
    }

    /// <summary>
    /// <c>1.82 sec</c> when the duration is at least one second, otherwise <c>420 ms</c>.
    /// </summary>
    public static string FormatDuration(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            duration = TimeSpan.Zero;
        }

        if (duration.TotalSeconds >= 1)
        {
            return $"{duration.TotalSeconds:0.00} sec";
        }

        return $"{Math.Max(0, (int)Math.Round(duration.TotalMilliseconds))} ms";
    }

    /// <summary>
    /// One-line memory summary, or empty when nothing useful was sampled.
    /// </summary>
    public static string FormatMemory(MemorySnapshot memory)
    {
        ArgumentNullException.ThrowIfNull(memory);

        var used = memory.WorkingSetBytes ?? memory.ManagedBytes;
        if (used is null)
        {
            return string.Empty;
        }

        var line = $"Memory            {MemorySnapshot.FormatBytes(used.Value)}";
        if (memory.AvailableBytes is > 0)
        {
            line += $" used  /  {MemorySnapshot.FormatBytes(memory.AvailableBytes.Value)} avail";
        }

        if (memory.Pressure is not MemoryPressureKind.Unknown and not MemoryPressureKind.Normal)
        {
            line += $"  ({memory.Pressure})";
        }

        return line;
    }
}
