using System.IO;
using System.Security;
using System.Windows;
using DesktopWorldClock.App.CityPicker;
using DesktopWorldClock.App.Clock;
using DesktopWorldClock.App.Diagnostics;
using DesktopWorldClock.App.Ui;
using DesktopWorldClock.Core.Cities;
using DesktopWorldClock.Core.Settings;
using DesktopWorldClock.Core.Time;

namespace DesktopWorldClock.App.Shell;

/// <summary>Owns the app's windows, tray icon and settings, and implements every menu command.</summary>
internal sealed class ShellController : IDisposable
{
    private readonly SettingsStore _store;
    private readonly TimeProvider _time;
    private readonly CityTimeCalculator _calculator = new();
    private readonly ClockViewModel _clock = new();
    private readonly ClockTicker _ticker;

    private AppSettings _settings = AppSettings.Default;
    private City _city = CityCatalog.Default;
    private ClockWindow? _window;
    private TrayIcon? _tray;
    private CityPickerWindow? _picker;

    public ShellController(SettingsStore store, TimeProvider time)
    {
        _store = store;
        _time = time;
        _ticker = new ClockTicker(time);
        _ticker.Tick += Render;
    }

    public void Start()
    {
        _settings = LoadSettings();
        _city = CityCatalog.FindById(_settings.Clocks[0].CityId) ?? CityCatalog.Default;
        RepairAutoStart();

        var menu = BuildMenu();
        _window = new ClockWindow(_clock, menu) { Topmost = _settings.IsTopmost };
        _window.PlacementCommitted += placement => SaveSettings(_settings with { Placement = placement });
        _window.IsVisibleChanged += (_, _) => SyncTickerWithVisibility();
        WindowPositioner.Place(_window, _settings.Placement);

        _tray = new TrayIcon(menu, ToggleClock);
        _window.Show();
    }

    public void ShowClock()
    {
        if (_window is null)
        {
            return;
        }

        _window.Show();
        // Flipping Topmost raises the window above others even when Windows denies foreground activation.
        _window.Topmost = true;
        _window.Topmost = _settings.IsTopmost;
        _window.Activate();
    }

    public void Dispose()
    {
        _ticker.Dispose();
        _tray?.Dispose();
    }

    private IReadOnlyList<MenuEntry> BuildMenu() =>
    [
        new MenuCommand("选择城市…", OpenCityPicker),
        new MenuCommand("始终置顶", ToggleTopmost, () => _settings.IsTopmost),
        new MenuCommand("开机自启动", ToggleAutoStart, () => AutoStartRegistration.IsEnabled),
        new MenuCommand("显示时钟", ToggleClock, () => _window?.IsVisible == true),
        new MenuSeparator(),
        new MenuCommand("退出", Exit),
    ];

    private void Render(DateTimeOffset now) =>
        _clock.Update(_city, ClockFormatter.Format(_calculator.Calculate(_city, now)));

    private void SyncTickerWithVisibility()
    {
        if (_window?.IsVisible == true)
        {
            _ticker.Start();
        }
        else
        {
            _ticker.Stop();
        }
    }

    private void ToggleClock()
    {
        if (_window?.IsVisible == true)
        {
            _window.Hide();
        }
        else
        {
            ShowClock();
        }
    }

    private void ToggleTopmost()
    {
        SaveSettings(_settings with { IsTopmost = !_settings.IsTopmost });
        if (_window is not null)
        {
            _window.Topmost = _settings.IsTopmost;
        }
    }

    private static void ToggleAutoStart()
    {
        if (Environment.ProcessPath is not { } executablePath)
        {
            ErrorReporter.Log("Auto-start toggle skipped: the executable path is unavailable.");
            return;
        }

        try
        {
            AutoStartRegistration.SetEnabled(!AutoStartRegistration.IsEnabled, executablePath);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException or IOException)
        {
            ErrorReporter.Log("Changing the auto-start registration failed.", ex);
        }
    }

    private static void RepairAutoStart()
    {
        if (Environment.ProcessPath is not { } executablePath)
        {
            return;
        }

        try
        {
            AutoStartRegistration.RepairIfTargetMissing(executablePath);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException or IOException)
        {
            ErrorReporter.Log("Repairing the auto-start registration failed.", ex);
        }
    }

    private void OpenCityPicker()
    {
        if (_picker is not null)
        {
            _picker.Activate();
            return;
        }

        _picker = new CityPickerWindow(_calculator, _time.GetUtcNow(), _city);
        _picker.CitySelected += SelectCity;
        _picker.Closed += (_, _) => _picker = null;
        _picker.Show();
        _picker.Activate();
    }

    private void SelectCity(City city)
    {
        _city = city;
        SaveSettings(_settings with { Clocks = [new ClockSettings(city.Id)] });
        Render(_time.GetUtcNow());
        // Deferred until the picker has closed; otherwise Windows re-activates the previous app over the clock.
        _window?.Dispatcher.BeginInvoke(ShowClock);
    }

    private void Exit()
    {
        _picker?.Close();
        Application.Current.Shutdown();
    }

    private AppSettings LoadSettings()
    {
        var result = _store.Load();
        if (result.Status is SettingsLoadStatus.RecoveredFromCorruptFile or SettingsLoadStatus.Unreadable)
        {
            ErrorReporter.Log($"Settings {result.Status}; using defaults. Backup: {result.BackupPath ?? "none"}", result.Error);
        }

        return result.Settings;
    }

    /// <summary>Saved on every change, so nothing is lost if the process is killed.</summary>
    private void SaveSettings(AppSettings settings)
    {
        _settings = settings;
        try
        {
            _store.Save(settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ErrorReporter.Log($"Saving settings to '{_store.FilePath}' failed; the change applies to this session only.", ex);
        }
    }
}
