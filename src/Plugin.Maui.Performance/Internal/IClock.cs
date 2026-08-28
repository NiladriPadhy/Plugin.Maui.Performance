namespace Plugin.Maui.Performance;

interface IClock
{
    DateTimeOffset UtcNow { get; }

    long GetTimestamp();

    TimeSpan GetElapsedTime(long startingTimestamp);
}
