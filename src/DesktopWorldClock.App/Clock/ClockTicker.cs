using System.Windows.Threading;

namespace DesktopWorldClock.App.Clock;

/// <summary>
/// Fires just after every whole second. Each tick re-reads the current time, so sleep/resume and system clock
/// changes self-correct within a second instead of drifting.
/// </summary>
internal sealed class ClockTicker : IDisposable
{
    private static readonly TimeSpan BoundaryMargin = TimeSpan.FromMilliseconds(5);

    private readonly TimeProvider _time;
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Background);

    public ClockTicker(TimeProvider time)
    {
        _time = time;
        _timer.Tick += (_, _) => FireAndRearm();
    }

    public event Action<DateTimeOffset>? Tick;

    public bool IsRunning => _timer.IsEnabled;

    public void Start()
    {
        if (!IsRunning)
        {
            FireAndRearm();
        }
    }

    public void Stop() => _timer.Stop();

    public void Dispose() => Stop();

    private void FireAndRearm()
    {
        _timer.Stop();
        var now = _time.GetUtcNow();
        Tick?.Invoke(now);
        _timer.Interval = TimeSpan.FromMilliseconds(1000 - now.Millisecond) + BoundaryMargin;
        _timer.Start();
    }
}
