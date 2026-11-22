using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using DesktopWorldClock.Core.Cities;

namespace DesktopWorldClock.Core.Settings;

public enum SettingsLoadStatus
{
    Loaded,
    NotFound,

    /// <summary>The file was invalid; it was moved aside and defaults are used.</summary>
    RecoveredFromCorruptFile,

    /// <summary>The file exists but could not be read (locked, no permission); defaults are used.</summary>
    Unreadable,
}

public sealed record SettingsLoadResult(
    AppSettings Settings,
    SettingsLoadStatus Status,
    string? BackupPath = null,
    Exception? Error = null);

/// <summary>Reads and writes <see cref="AppSettings"/> as a JSON file. Loading never throws.</summary>
public sealed class SettingsStore(string filePath)
{
    public static string DefaultFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DesktopWorldClock",
        "settings.json");

    public string FilePath { get; } = filePath;

    public SettingsLoadResult Load()
    {
        if (!File.Exists(FilePath))
        {
            return new SettingsLoadResult(AppSettings.Default, SettingsLoadStatus.NotFound);
        }

        string json;
        try
        {
            json = File.ReadAllText(FilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new SettingsLoadResult(AppSettings.Default, SettingsLoadStatus.Unreadable, Error: ex);
        }

        try
        {
            var parsed = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.AppSettings)
                ?? throw new JsonException("Settings file contains null.");
            return new SettingsLoadResult(Normalize(parsed), SettingsLoadStatus.Loaded);
        }
        catch (JsonException ex)
        {
            return RecoverFromCorruptFile(ex);
        }
    }

    /// <summary>Writes to a temporary file first so a crash mid-write cannot leave a half-written settings file.</summary>
    public void Save(AppSettings settings)
    {
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = FilePath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(settings, SettingsJsonContext.Default.AppSettings));
        File.Move(tempPath, FilePath, overwrite: true);
    }

    private SettingsLoadResult RecoverFromCorruptFile(JsonException error)
    {
        var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        var backupPath = Path.ChangeExtension(FilePath, $".corrupt-{stamp}.json");
        try
        {
            File.Move(FilePath, backupPath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new SettingsLoadResult(AppSettings.Default, SettingsLoadStatus.Unreadable,
                Error: new IOException($"Settings file '{FilePath}' is corrupt and could not be moved aside.", ex));
        }

        return new SettingsLoadResult(AppSettings.Default, SettingsLoadStatus.RecoveredFromCorruptFile, backupPath, error);
    }

    /// <summary>JSON may contain nulls or unknown city ids even where the C# types say otherwise.</summary>
    private static AppSettings Normalize(AppSettings parsed)
    {
        IReadOnlyList<ClockSettings> clocks = parsed.Clocks is { Count: > 0 }
            ? [.. parsed.Clocks.Select(NormalizeClock)]
            : AppSettings.Default.Clocks;
        return parsed with { SchemaVersion = AppSettings.CurrentSchemaVersion, Clocks = clocks };
    }

    private static ClockSettings NormalizeClock(ClockSettings? clock) =>
        clock?.CityId is { } cityId && CityCatalog.FindById(cityId) is not null
            ? clock
            : new ClockSettings(CityCatalog.DefaultCityId);
}

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(AppSettings))]
internal sealed partial class SettingsJsonContext : JsonSerializerContext;
