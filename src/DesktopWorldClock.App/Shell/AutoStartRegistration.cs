using System.IO;
using DesktopWorldClock.Core.Settings;
using Microsoft.Win32;

namespace DesktopWorldClock.App.Shell;

/// <summary>Registers the app under the per-user Run key so it starts at sign-in without elevation.</summary>
internal static class AutoStartRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "DesktopWorldClock";

    public static bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            return key?.GetValue(ValueName) is string;
        }
    }

    public static void SetEnabled(bool isEnabled, string executablePath)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true)
            ?? throw new UnauthorizedAccessException($"Cannot open the registry key '{RunKeyPath}'.");
        if (isEnabled)
        {
            key.SetValue(ValueName, StartupCommand.Build(executablePath));
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }

    /// <summary>
    /// Follows the executable when it moves (for example after reinstalling elsewhere). The path is only
    /// rewritten when the registered one no longer exists, so running a build from another folder does not
    /// take over the entry.
    /// </summary>
    public static void RepairIfTargetMissing(string executablePath)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
        if (key?.GetValue(ValueName) is not string command)
        {
            return;
        }

        var registeredPath = StartupCommand.GetExecutablePath(command);
        if (registeredPath is null || File.Exists(registeredPath))
        {
            return;
        }

        SetEnabled(isEnabled: true, executablePath);
    }
}
