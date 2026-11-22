using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DesktopWorldClock.App.Ui;
using DesktopWorldClock.Core.Cities;
using DesktopWorldClock.Core.Time;

namespace DesktopWorldClock.App.CityPicker;

/// <summary>
/// Popover for choosing the clock's city. Type to filter, arrow keys to move, Enter or click to pick,
/// Esc or clicking elsewhere to dismiss.
/// </summary>
internal sealed partial class CityPickerWindow : Window
{
    private readonly CityTimeCalculator _calculator;
    private readonly DateTimeOffset _openedAt;
    private bool _isClosing;

    public CityPickerWindow(CityTimeCalculator calculator, DateTimeOffset now, City currentCity)
    {
        InitializeComponent();
        _calculator = calculator;
        _openedAt = now;
        GlassBackdrop.Attach(this);

        ShowOptions(string.Empty);
        SelectCity(currentCity);

        SearchBox.TextChanged += (_, _) => ShowOptions(SearchBox.Text);
        PreviewKeyDown += OnPreviewKeyDown;
        CityList.PreviewMouseLeftButtonUp += OnListClicked;
        Deactivated += (_, _) => CloseOnce();
        Loaded += (_, _) => SearchBox.Focus();
    }

    public event Action<City>? CitySelected;

    protected override void OnClosing(CancelEventArgs e)
    {
        _isClosing = true;
        base.OnClosing(e);
    }

    private void ShowOptions(string query)
    {
        var options = CityOptions.Search(query, _openedAt, _calculator);
        CityList.ItemsSource = options;
        CityList.SelectedIndex = options.Count > 0 ? 0 : -1;
    }

    private void SelectCity(City city)
    {
        var match = CityList.Items.Cast<CityOption>().FirstOrDefault(option => option.City.Id == city.Id);
        if (match is null)
        {
            return;
        }

        CityList.SelectedItem = match;
        CityList.ScrollIntoView(match);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                CloseOnce();
                break;
            case Key.Enter:
                ConfirmSelection();
                break;
            case Key.Down:
                MoveSelection(1);
                break;
            case Key.Up:
                MoveSelection(-1);
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private void MoveSelection(int delta)
    {
        if (CityList.Items.Count == 0)
        {
            return;
        }

        CityList.SelectedIndex = Math.Clamp(CityList.SelectedIndex + delta, 0, CityList.Items.Count - 1);
        CityList.ScrollIntoView(CityList.SelectedItem);
    }

    private void OnListClicked(object sender, MouseButtonEventArgs e)
    {
        var item = FindAncestor<ListBoxItem>(e.OriginalSource as DependencyObject);
        if (item?.DataContext is CityOption option)
        {
            Confirm(option);
        }
    }

    private void ConfirmSelection()
    {
        if (CityList.SelectedItem is CityOption option)
        {
            Confirm(option);
        }
    }

    private void Confirm(CityOption option)
    {
        CitySelected?.Invoke(option.City);
        CloseOnce();
    }

    /// <summary>Closing deactivates the window, which would call Close again; the flag breaks that loop.</summary>
    private void CloseOnce()
    {
        if (_isClosing)
        {
            return;
        }

        _isClosing = true;
        Close();
    }

    private static T? FindAncestor<T>(DependencyObject? node)
        where T : DependencyObject
    {
        while (node is not null and not T)
        {
            node = VisualTreeHelper.GetParent(node);
        }

        return node as T;
    }
}
