using System.Drawing;
using System.Windows.Forms;
using DesktopWorldClock.App.Ui;
using WpfApplication = System.Windows.Application;

namespace DesktopWorldClock.App.Shell;

/// <summary>Notification-area icon: left click toggles the clock, right click opens the shared menu.</summary>
internal sealed class TrayIcon : IDisposable
{
    private static readonly Uri IconUri = new("pack://application:,,,/Assets/app.ico");

    private readonly Icon _icon;
    private readonly ContextMenuStrip _menu;
    private readonly NotifyIcon _notifyIcon;

    public TrayIcon(IReadOnlyList<MenuEntry> entries, Action onLeftClick)
    {
        _icon = LoadIcon();
        _menu = new ContextMenuStrip { RenderMode = ToolStripRenderMode.System };
        _menu.Opening += (_, e) =>
        {
            RebuildMenu(entries);
            e.Cancel = false;
        };
        _notifyIcon = new NotifyIcon
        {
            Icon = _icon,
            Text = "Desktop World Clock",
            ContextMenuStrip = _menu,
            Visible = true,
        };
        _notifyIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                onLeftClick();
            }
        };
    }

    /// <summary>Hides the icon first; otherwise a stale icon can linger in the tray until hovered.</summary>
    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        DisposeMenuItems();
        _menu.Dispose();
        _icon.Dispose();
    }

    private void RebuildMenu(IReadOnlyList<MenuEntry> entries)
    {
        DisposeMenuItems();
        foreach (var entry in entries)
        {
            _menu.Items.Add(entry is MenuCommand command ? CreateItem(command) : new ToolStripSeparator());
        }
    }

    private void DisposeMenuItems()
    {
        foreach (var item in _menu.Items.Cast<ToolStripItem>().ToList())
        {
            item.Dispose();
        }
    }

    private static ToolStripMenuItem CreateItem(MenuCommand command)
    {
        var item = new ToolStripMenuItem(command.Label) { Checked = command.IsChecked?.Invoke() ?? false };
        item.Click += (_, _) => command.Execute();
        return item;
    }

    private static Icon LoadIcon()
    {
        var resource = WpfApplication.GetResourceStream(IconUri)
            ?? throw new InvalidOperationException($"Tray icon resource '{IconUri}' is missing.");
        using var stream = resource.Stream;
        return new Icon(stream, SystemInformation.SmallIconSize);
    }
}
