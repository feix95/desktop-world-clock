using System.ComponentModel;
using DesktopWorldClock.Core.Cities;
using DesktopWorldClock.Core.Time;

namespace DesktopWorldClock.App.Clock;

/// <summary>Bindable text for one clock face. Raises change notifications only when a value actually changes.</summary>
internal sealed class ClockViewModel : INotifyPropertyChanged
{
    private string _cityName = string.Empty;
    private string _cityNameEn = string.Empty;
    private string _time = string.Empty;
    private string _date = string.Empty;
    private string _utcOffset = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CityName => _cityName;

    public string CityNameEn => _cityNameEn;

    public string Time => _time;

    public string Date => _date;

    public string UtcOffset => _utcOffset;

    public void Update(City city, ClockDisplay display)
    {
        Set(ref _cityName, city.NameZh, nameof(CityName));
        Set(ref _cityNameEn, city.NameEn, nameof(CityNameEn));
        Set(ref _time, display.Time, nameof(Time));
        Set(ref _date, display.Date, nameof(Date));
        Set(ref _utcOffset, display.UtcOffset, nameof(UtcOffset));
    }

    private void Set(ref string field, string value, string propertyName)
    {
        if (string.Equals(field, value, StringComparison.Ordinal))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
