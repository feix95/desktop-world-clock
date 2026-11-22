using DesktopWorldClock.Core.Cities;
using DesktopWorldClock.Core.Settings;

namespace DesktopWorldClock.Core.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "dwc-tests-" + Guid.NewGuid().ToString("N"));

    private string SettingsPath => Path.Combine(_directory, "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void MissingFileReturnsDefaults()
    {
        var result = new SettingsStore(SettingsPath).Load();
        Assert.Equal(SettingsLoadStatus.NotFound, result.Status);
        Assert.Equal(CityCatalog.DefaultCityId, result.Settings.Clocks[0].CityId);
    }

    [Fact]
    public void SaveThenLoadRoundTrips()
    {
        var store = new SettingsStore(SettingsPath);
        var saved = AppSettings.Default with
        {
            Clocks = [new ClockSettings("tokyo")],
            Placement = new WindowPlacement(120.5, 80),
            IsTopmost = true,
        };

        store.Save(saved);
        var result = store.Load();

        Assert.Equal(SettingsLoadStatus.Loaded, result.Status);
        Assert.Equal("tokyo", result.Settings.Clocks[0].CityId);
        Assert.Equal(saved.Placement, result.Settings.Placement);
        Assert.True(result.Settings.IsTopmost);
        Assert.False(File.Exists(SettingsPath + ".tmp"));
    }

    [Fact]
    public void CorruptFileIsMovedAsideAndDefaultsAreUsed()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(SettingsPath, "{ not json");

        var result = new SettingsStore(SettingsPath).Load();

        Assert.Equal(SettingsLoadStatus.RecoveredFromCorruptFile, result.Status);
        Assert.NotNull(result.BackupPath);
        Assert.True(File.Exists(result.BackupPath));
        Assert.False(File.Exists(SettingsPath));
        Assert.Equal(CityCatalog.DefaultCityId, result.Settings.Clocks[0].CityId);
    }

    [Theory]
    [InlineData("""{ "clocks": [ { "cityId": "atlantis" } ] }""")]
    [InlineData("""{ "clocks": null }""")]
    [InlineData("""{ "clocks": [] }""")]
    [InlineData("""{ "clocks": [ null ] }""")]
    public void InvalidClockEntriesFallBackToDefaultCity(string json)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(SettingsPath, json);

        var result = new SettingsStore(SettingsPath).Load();

        Assert.Equal(SettingsLoadStatus.Loaded, result.Status);
        Assert.Equal(CityCatalog.DefaultCityId, Assert.Single(result.Settings.Clocks).CityId);
    }
}
