using DesktopWorldClock.Core.Cities;

namespace DesktopWorldClock.Core.Time;

/// <summary>A city as listed in the picker, with its UTC offset at the time the list was built.</summary>
public sealed record CityOption(City City, string UtcOffset)
{
    public string NameZh => City.NameZh;

    public string NameEn => City.NameEn;
}

public static class CityOptions
{
    public static IReadOnlyList<CityOption> Search(string query, DateTimeOffset moment, CityTimeCalculator calculator) =>
    [
        .. CityCatalog.Search(query).Select(city => new CityOption(
            city,
            ClockFormatter.FormatUtcOffset(calculator.Calculate(city, moment).LocalTime.Offset))),
    ];
}
