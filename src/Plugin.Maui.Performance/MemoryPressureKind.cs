namespace Plugin.Maui.Performance;

/// <summary>
/// Coarse memory pressure reported by the OS or inferred from free RAM.
/// </summary>
public enum MemoryPressureKind
{
    /// <summary>Not measured.</summary>
    Unknown = 0,

    /// <summary>Plenty of memory.</summary>
    Normal,

    /// <summary>The OS has asked apps to trim, or free RAM is low.</summary>
    Warning,

    /// <summary>The process is close to being killed.</summary>
    Critical
}
