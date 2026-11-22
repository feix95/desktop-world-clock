using DesktopWorldClock.Core.Cities;

namespace DesktopWorldClock.Core.Settings;

/// <summary>User preferences persisted between runs.</summary>
public sealed record AppSettings
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    /// <summary>A list so more clocks can be added later without a schema change; the prototype shows the first.</summary>
    public IReadOnlyList<ClockSettings> Clocks { get; init; } = [new ClockSettings(CityCatalog.DefaultCityId)];

    /// <summary><c>null</c> until the user has placed the window, meaning "use the default position".</summary>
    public WindowPlacement? Placement { get; init; }

    public bool IsTopmost { get; init; }

    public static AppSettings Default { get; } = new();
}

public sealed record ClockSettings(string CityId);

/// <summary>Window position in device-independent pixels.</summary>
public sealed record WindowPlacement(double Left, double Top);
