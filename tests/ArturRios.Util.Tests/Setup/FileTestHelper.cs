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