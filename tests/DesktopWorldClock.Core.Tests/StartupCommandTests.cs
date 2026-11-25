using DesktopWorldClock.Core.Settings;

namespace DesktopWorldClock.Core.Tests;

public sealed class StartupCommandTests
{
    [Fact]
    public void BuildQuotesPathWithSpaces()
    {
        var command = StartupCommand.Build(@"C:\Program Files\Desktop World Clock\DesktopWorldClock.exe");

        Assert.Equal("\"C:\\Program Files\\Desktop World Clock\\DesktopWorldClock.exe\"", command);
    }

    [Fact]
    public void QuotedCommandRoundTrips()
    {
        const string Path = @"C:\Users\someone\AppData\Local\Clock\DesktopWorldClock.exe";

        Assert.Equal(Path, StartupCommand.GetExecutablePath(StartupCommand.Build(Path)));
    }

    [Theory]
    [InlineData("\"C:\\Tools\\Clock\\DesktopWorldClock.exe\" --tray", @"C:\Tools\Clock\DesktopWorldClock.exe")]
    [InlineData("\"C:\\Tools\\Clock\\DesktopWorldClock.exe\"", @"C:\Tools\Clock\DesktopWorldClock.exe")]
    [InlineData("C:\\Tools\\Clock\\DesktopWorldClock.exe --tray", @"C:\Tools\Clock\DesktopWorldClock.exe")]
    [InlineData("  C:\\Tools\\Clock\\DesktopWorldClock.exe  ", @"C:\Tools\Clock\DesktopWorldClock.exe")]
    public void ParsesExecutablePath(string command, string expected)
    {
        Assert.Equal(expected, StartupCommand.GetExecutablePath(command));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\"")]
    [InlineData("\"\"")]
    [InlineData("\"C:\\Tools\\Clock\\DesktopWorldClock.exe")]
    public void UnparsableCommandReturnsNull(string? command)
    {
        Assert.Null(StartupCommand.GetExecutablePath(command));
    }
}
