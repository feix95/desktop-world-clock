namespace DesktopWorldClock.Core.Tests;

/// <summary>Keeps every hand-written source file small enough to read in one pass.</summary>
public class SourceFileLengthTests
{
    private const int MaxNonBlankLines = 300;

    [Fact]
    public void SourceFilesStayUnderLineLimit()
    {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src");
        var oversized = Directory
            .EnumerateFiles(sourceRoot, "*.*", SearchOption.AllDirectories)
            .Where(IsHandWrittenSource)
            .Select(path => (Path: path, Lines: File.ReadLines(path).Count(line => line.Trim().Length > 0)))
            .Where(file => file.Lines > MaxNonBlankLines)
            .Select(file => $"{Path.GetRelativePath(sourceRoot, file.Path)} ({file.Lines} lines)")
            .ToList();

        Assert.True(oversized.Count == 0, "Files over the limit: " + string.Join(", ", oversized));
    }

    private static bool IsHandWrittenSource(string path)
    {
        var isSourceFile = path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase);
        var separator = Path.DirectorySeparatorChar;
        var isBuildOutput = path.Contains($"{separator}obj{separator}", StringComparison.OrdinalIgnoreCase)
            || path.Contains($"{separator}bin{separator}", StringComparison.OrdinalIgnoreCase);
        return isSourceFile && !isBuildOutput;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DesktopWorldClock.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
