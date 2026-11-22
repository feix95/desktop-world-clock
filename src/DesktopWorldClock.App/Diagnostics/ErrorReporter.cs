using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace DesktopWorldClock.App.Diagnostics;

/// <summary>Central place for logging failures. Unhandled UI errors are logged, shown once, and end the app cleanly.</summary>
internal static class ErrorReporter
{
    private const long MaxLogBytes = 1_000_000;
    private static readonly Lock LogGate = new();

    public static string LogPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DesktopWorldClock",
        "logs",
        "app.log");

    public static void Install(Application app)
    {
        app.DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException +=
            (_, e) => Log("Unhandled exception", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log("Unobserved task exception", e.Exception);
            e.SetObserved();
        };
    }

    public static void Log(string context, Exception? error = null)
    {
        var entry = $"{DateTimeOffset.Now:O} {context}{Environment.NewLine}{error}{Environment.NewLine}";
        try
        {
            lock (LogGate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                RotateIfLarge();
                File.AppendAllText(LogPath, entry);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The log file is the last resort; if it is unwritable, the debugger output is all that is left.
            Trace.WriteLine($"Failed to write log: {ex.Message}{Environment.NewLine}{entry}");
        }
    }

    private static void RotateIfLarge()
    {
        var file = new FileInfo(LogPath);
        if (file.Exists && file.Length > MaxLogBytes)
        {
            File.Move(LogPath, LogPath + ".old", overwrite: true);
        }
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log("Unhandled UI exception", e.Exception);
        e.Handled = true;
        MessageBox.Show(
            $"时钟遇到了问题, 即将关闭。\n错误记录在: {LogPath}",
            "Desktop World Clock",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        Application.Current.Shutdown();
    }
}
