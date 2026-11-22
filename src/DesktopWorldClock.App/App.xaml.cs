using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DesktopWorldClock.App.Diagnostics;
using DesktopWorldClock.App.Shell;
using DesktopWorldClock.Core.Settings;

namespace DesktopWorldClock.App;

/// <summary>Composition root: wires the single-instance guard, error reporting and the shell together.</summary>
[SuppressMessage("Design", "CA1001", Justification = "Application has no Dispose; owned resources are released in OnExit.")]
public partial class App : Application
{
    private SingleInstanceGuard? _instanceGuard;
    private ShellController? _shell;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ErrorReporter.Install(this);
        // A clock redraws a few glyphs per second; skipping GPU rendering avoids loading the graphics driver
        // (well over 100 MB mapped on NVIDIA) for no visible difference. Desktop blur is done by DWM either way.
        RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

        _instanceGuard = SingleInstanceGuard.TryAcquire();
        if (_instanceGuard is null)
        {
            SingleInstanceGuard.SignalExistingInstance();
            Shutdown();
            return;
        }

        _shell = new ShellController(new SettingsStore(SettingsStore.DefaultFilePath), TimeProvider.System);
        _instanceGuard.ActivationRequested += () => Dispatcher.BeginInvoke(_shell.ShowClock);
        _shell.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _shell?.Dispose();
        _instanceGuard?.Dispose();
        base.OnExit(e);
    }
}
