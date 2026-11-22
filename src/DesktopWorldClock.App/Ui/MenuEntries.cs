using System.Windows;
using System.Windows.Controls;

namespace DesktopWorldClock.App.Ui;

/// <summary>One menu definition shared by the tray menu and the clock's right-click menu.</summary>
internal abstract record MenuEntry;

internal sealed record MenuCommand(string Label, Action Execute, Func<bool>? IsChecked = null) : MenuEntry;

internal sealed record MenuSeparator : MenuEntry;

internal static class MenuFactory
{
    private const string SeparatorStyleKey = "MenuSeparator";

    public static ContextMenu CreateContextMenu(IEnumerable<MenuEntry> entries)
    {
        var menu = new ContextMenu();
        foreach (var entry in entries)
        {
            menu.Items.Add(entry is MenuCommand command ? CreateItem(command) : CreateSeparator());
        }

        return menu;
    }

    private static MenuItem CreateItem(MenuCommand command)
    {
        var item = new MenuItem { Header = command.Label, IsChecked = command.IsChecked?.Invoke() ?? false };
        item.Click += (_, _) => command.Execute();
        return item;
    }

    // ContextMenu forces its own separator style key, so the themed style must be assigned explicitly.
    private static Separator CreateSeparator() =>
        new() { Style = (Style)Application.Current.FindResource(SeparatorStyleKey) };
}
