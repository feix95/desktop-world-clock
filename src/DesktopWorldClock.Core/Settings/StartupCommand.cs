namespace DesktopWorldClock.Core.Settings;

/// <summary>
/// Builds and parses the command line stored under the per-user Run registry key.
/// The path is quoted because the install location may contain spaces.
/// </summary>
public static class StartupCommand
{
    public static string Build(string executablePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        return $"\"{executablePath}\"";
    }

    /// <summary>Returns the executable path from a Run value, or <c>null</c> when the value cannot be parsed.</summary>
    public static string? GetExecutablePath(string? command)
    {
        var trimmed = command?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed[0] != '"')
        {
            var separator = trimmed.IndexOf(' ');
            return separator < 0 ? trimmed : trimmed[..separator];
        }

        var closing = trimmed.IndexOf('"', 1);
        return closing > 1 ? trimmed[1..closing] : null;
    }
}
