using DesktopWorldClock.Core.Cities;
using DesktopWorldClock.Core.Time;

namespace DesktopWorldClock.Core.Tests;

public class CityCatalogTests
{
    [Fact]
    public void IdsAreUnique()
    {
        var ids = CityCatalog.All.Select(city => city.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void EveryCityTimeZoneResolvesOnThisMachine()
    {
        var calculator = new CityTimeCalculator();
        foreach (var city in CityCatalog.All)
        {
            var exception = Record.Exception(() => calculator.Calculate(city, DateTimeOffset.UtcNow));
            Assert.Null(exception);
        }
    }

    [Fact]
    public void DefaultCityExists() => Assert.Equal(CityCatalog.DefaultCityId, CityCatalog.Default.Id);

    [Theory]
    [InlineData("纽约", "new-york")]
    [InlineData("new york", "new-york")]
    [InlineData("LONDON", "london")]
    [InlineData("  东京 ", "tokyo")]
    public void SearchMatchesChineseAndEnglishNames(string query, string expectedId)
    {
        var results = CityCatalog.Search(query);
        Assert.Contains(results, city => city.Id == expectedId);
    }

    [Fact]
    public void EmptySearchReturnsAllCities() => Assert.Equal(CityCatalog.All.Count, CityCatalog.Search("   ").Count);

    [Fact]
    public void SearchWithoutMatchReturnsEmpty() => Assert.Empty(CityCatalog.Search("atlantis"));

    [Fact]
    public void FindByUnknownIdReturnsNull() => Assert.Null(CityCatalog.FindById("atlantis"));
}
