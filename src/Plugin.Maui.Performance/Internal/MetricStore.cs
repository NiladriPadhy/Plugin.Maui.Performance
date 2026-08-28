namespace Plugin.Maui.Performance;

sealed class MetricStore
{
    readonly object _gate = new();
    readonly List<PerformanceMetric> _items = [];
    readonly int _max;

    public MetricStore(int max) => _max = Math.Max(1, max);

    public void Add(PerformanceMetric metric)
    {
        lock (_gate)
        {
            _items.Add(metric);
            var overflow = _items.Count - _max;
            if (overflow > 0)
            {
                _items.RemoveRange(0, overflow);
            }
        }
    }

    public IReadOnlyList<PerformanceMetric> Snapshot()
    {
        lock (_gate)
        {
            return _items.ToArray();
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _items.Clear();
        }
    }
}
