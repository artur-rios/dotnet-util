namespace ArturRios.Util.Tests.Setup;

public static class FileTestHelper
{
    public static string CreateTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

        File.WriteAllText(path, content);

        return path;
    }
}

/// <summary>
/// A temp file that deletes itself, so a test can be a handful of lines instead of a try/finally block.
/// </summary>
public sealed class TempFile : IDisposable
{
    public TempFile(string content)
    {
        Path = FileTestHelper.CreateTempFile(content);
    }

    public string Path { get; }

    /// <summary>
    /// Builds a temp file from one line per argument, so test data reads as the file it becomes.
    /// </summary>
    public static TempFile WithLines(params string[] lines) => new(string.Join(Environment.NewLine, lines));

    public void Dispose()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
    }

    public static implicit operator string(TempFile file) => file.Path;
}
