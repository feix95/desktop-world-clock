using DesktopWorldClock.Core.Cities;

namespace DesktopWorldClock.Core.Time;

/// <summary>A moment expressed in a city's local time; <c>LocalTime.Offset</c> is the city's current UTC offset.</summary>
public sealed record CityTime(City City, DateTimeOffset LocalTime);

/// <summary>
/// Converts an absolute moment into a city's local time. The machine's own time zone setting is never used;
/// daylight saving rules come from the time zone database shipped with the OS.
/// </summary>
public sealed class CityTimeCalculator
{
    private readonly Dictionary<string, TimeZoneInfo> _zones = new(StringComparer.Ordinal);

    public CityTime Calculate(City city, DateTimeOffset moment) =>
        new(city, TimeZoneInfo.ConvertTime(moment, ResolveZone(city)));

    private TimeZoneInfo ResolveZone(City city)
    {
        if (_zones.TryGetValue(city.TimeZoneId, out var cached))
        {
            return cached;
        }

        TimeZoneInfo zone;
        try
        {
            zone = TimeZoneInfo.FindSystemTimeZoneById(city.TimeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new InvalidOperationException(
                $"Cannot resolve time zone '{city.TimeZoneId}' for city '{city.Id}'.", ex);
        }

        _zones[city.TimeZoneId] = zone;
        return zone;
    }
}
