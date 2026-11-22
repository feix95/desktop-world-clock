using System.Runtime.InteropServices;

namespace DesktopWorldClock.App.Shell;

/// <summary>
/// Ensures only one clock process runs. A second launch signals the first one to show its window, then exits.
/// </summary>
internal sealed class SingleInstanceGuard : IDisposable
{
    private const string MutexName = @"Local\DesktopWorldClock.SingleInstance";
    private const string ActivateEventName = @"Local\DesktopWorldClock.Activate";

    private readonly Mutex _mutex;
    private readonly EventWaitHandle _activateEvent;
    private readonly RegisteredWaitHandle _registration;

    private SingleInstanceGuard(Mutex mutex)
    {
        _mutex = mutex;
        _activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ActivateEventName);
        _registration = ThreadPool.RegisterWaitForSingleObject(
            _activateEvent, (_, _) => ActivationRequested?.Invoke(), null, Timeout.Infinite, executeOnlyOnce: false);
    }

    /// <summary>Raised on a thread-pool thread; marshal to the UI thread before touching windows.</summary>
    public event Action? ActivationRequested;

    /// <returns>The guard for the first instance, or <c>null</c> when another instance already runs.</returns>
    public static SingleInstanceGuard? TryAcquire()
    {
        var mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
        if (createdNew)
        {
            return new SingleInstanceGuard(mutex);
        }

        mutex.Dispose();
        return null;
    }

    public static void SignalExistingInstance()
    {
        if (!EventWaitHandle.TryOpenExisting(ActivateEventName, out var activateEvent))
        {
            return;
        }

        // The user launched this process, so it holds foreground rights; hand them to the running instance,
        // otherwise Windows refuses to let it come to the front.
        NativeMethods.AllowSetForegroundWindow(NativeMethods.AsfwAny);
        using (activateEvent)
        {
            activateEvent.Set();
        }
    }

    public void Dispose()
    {
        _registration.Unregister(null);
        _activateEvent.Dispose();
        _mutex.ReleaseMutex();
        _mutex.Dispose();
    }

    private static class NativeMethods
    {
        public const int AsfwAny = -1;

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllowSetForegroundWindow(int processId);
    }
}
