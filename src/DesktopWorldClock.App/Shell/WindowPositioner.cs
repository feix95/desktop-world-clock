using System.Windows;
using DesktopWorldClock.Core.Settings;

namespace DesktopWorldClock.App.Shell;

/// <summary>
/// Restores a saved window position, or places the window at the top-right of the work area when there is
/// no saved position or it would be off-screen (for example after a monitor was unplugged).
/// </summary>
internal static class WindowPositioner
{
    private const double ScreenMargin = 32;
    private const double MinVisiblePart = 48;

    public static void Place(Window window, WindowPlacement? saved)
    {
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        if (saved is not null && IsOnScreen(saved))
        {
            window.Left = saved.Left;
            window.Top = saved.Top;
            return;
        }

        // Width is only known after layout because the window sizes to its content.
        window.Left = SystemParameters.WorkArea.Right;
        window.Top = SystemParameters.WorkArea.Top + ScreenMargin;
        window.ContentRendered += (_, _) => window.Left = SystemParameters.WorkArea.Right - window.ActualWidth - ScreenMargin;
    }

    private static bool IsOnScreen(WindowPlacement placement)
    {
        var screen = new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);
        return screen.Contains(new Point(placement.Left + MinVisiblePart, placement.Top + MinVisiblePart));
    }
}
