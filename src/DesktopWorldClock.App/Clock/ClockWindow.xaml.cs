using System.Windows;
using System.Windows.Input;
using DesktopWorldClock.App.Ui;
using DesktopWorldClock.Core.Settings;

namespace DesktopWorldClock.App.Clock;

/// <summary>The floating clock card. Drag with the left button; right click for the menu.</summary>
internal sealed partial class ClockWindow : Window
{
    private readonly IReadOnlyList<MenuEntry> _menu;

    public ClockWindow(ClockViewModel viewModel, IReadOnlyList<MenuEntry> menu)
    {
        InitializeComponent();
        DataContext = viewModel;
        _menu = menu;
        GlassBackdrop.Attach(this);
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseRightButtonUp += OnMouseRightButtonUp;
    }

    /// <summary>Raised after the user finishes dragging the window.</summary>
    public event Action<WindowPlacement>? PlacementCommitted;

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed)
        {
            return;
        }

        DragMove();
        PlacementCommitted?.Invoke(new WindowPlacement(Left, Top));
    }

    private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        var contextMenu = MenuFactory.CreateContextMenu(_menu);
        contextMenu.PlacementTarget = this;
        contextMenu.IsOpen = true;
        e.Handled = true;
    }
}
