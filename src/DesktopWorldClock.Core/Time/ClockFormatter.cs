using System.Globalization;

namespace DesktopWorldClock.Core.Time;

/// <summary>Text shown on a clock face.</summary>
public sealed record ClockDisplay(string Time, string Date, string UtcOffset);

public static class ClockFormatter
{
    private static readonly CultureInfo DateCulture = CultureInfo.GetCultureInfo("zh-CN");

    public static ClockDisplay Format(CityTime cityTime)
    {
        var local = cityTime.LocalTime;
        return new ClockDisplay(
            local.ToString("HH:mm", CultureInfo.InvariantCulture),
            local.ToString("M月d日 dddd", DateCulture),
            FormatUtcOffset(local.Offset));
    }

    /// <example><c>UTC+8</c>, <c>UTC+5:45</c>, <c>UTC-3:30</c>, <c>UTC+0</c></example>
    public static string FormatUtcOffset(TimeSpan offset)
    {
        var sign = offset < TimeSpan.Zero ? "-" : "+";
        var magnitude = offset.Duration();
        var hours = (int)magnitude.TotalHours;
        return magnitude.Minutes == 0
            ? string.Create(CultureInfo.InvariantCulture, $"UTC{sign}{hours}")
            : string.Create(CultureInfo.InvariantCulture, $"UTC{sign}{hours}:{magnitude.Minutes:00}");
    }
}
