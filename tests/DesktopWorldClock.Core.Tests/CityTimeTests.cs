using DesktopWorldClock.Core.Cities;
using DesktopWorldClock.Core.Time;

namespace DesktopWorldClock.Core.Tests;

public class CityTimeTests
{
    private readonly CityTimeCalculator _calculator = new();

    [Theory]
    [InlineData("shanghai", "2026-11-22T00:00:00Z", "08:00", "UTC+8")]
    [InlineData("kathmandu", "2026-11-22T00:00:00Z", "05:45", "UTC+5:45")]
    [InlineData("new-delhi", "2026-11-22T00:00:00Z", "05:30", "UTC+5:30")]
    [InlineData("new-york", "2026-01-15T12:00:00Z", "07:00", "UTC-5")]
    [InlineData("new-york", "2026-07-01T12:00:00Z", "08:00", "UTC-4")]
    [InlineData("london", "2026-01-15T12:00:00Z", "12:00", "UTC+0")]
    [InlineData("london", "2026-07-01T12:00:00Z", "13:00", "UTC+1")]
    [InlineData("sydney", "2026-01-15T00:00:00Z", "11:00", "UTC+11")]
    [InlineData("sydney", "2026-07-01T00:00:00Z", "10:00", "UTC+10")]
    public void ConvertsToCityTimeIncludingDaylightSaving(string cityId, string utc, string expectedTime, string expectedOffset)
    {
        var city = CityCatalog.FindById(cityId)!;
        var moment = DateTimeOffset.Parse(utc, System.Globalization.CultureInfo.InvariantCulture);

        var display = ClockFormatter.Format(_calculator.Calculate(city, moment));

        Assert.Equal(expectedTime, display.Time);
        Assert.Equal(expectedOffset, display.UtcOffset);
    }

    [Fact]
    public void ResultDoesNotDependOnMachineTimeZone()
    {
        var moment = new DateTimeOffset(2026, 11, 22, 4, 30, 0, TimeSpan.FromHours(-7));
        var cityTime = _calculator.Calculate(CityCatalog.FindById("tokyo")!, moment);
        Assert.Equal(new DateTime(2026, 11, 22, 20, 30, 0), cityTime.LocalTime.DateTime);
    }

    [Fact]
    public void FormatsChineseDate()
    {
        var cityTime = _calculator.Calculate(CityCatalog.Default, new DateTimeOffset(2026, 11, 22, 1, 5, 0, TimeSpan.Zero));
        var display = ClockFormatter.Format(cityTime);
        Assert.Equal("09:05", display.Time);
        Assert.Equal("11月22日 星期日", display.Date);
    }

    [Theory]
    [InlineData(-210, "UTC-3:30")]
    [InlineData(0, "UTC+0")]
    [InlineData(765, "UTC+12:45")]
    public void FormatsUtcOffset(int minutes, string expected) =>
        Assert.Equal(expected, ClockFormatter.FormatUtcOffset(TimeSpan.FromMinutes(minutes)));

    [Fact]
    public void UnknownTimeZoneFailsWithContext()
    {
        var city = new City("nowhere", "无", "Nowhere", "Nowhere/Nowhere");
        var error = Assert.Throws<InvalidOperationException>(() => _calculator.Calculate(city, DateTimeOffset.UtcNow));
        Assert.Contains("Nowhere/Nowhere", error.Message, StringComparison.Ordinal);
    }
}
