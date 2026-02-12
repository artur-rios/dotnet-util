using System.Text.Json;
using ArturRios.Util.IO;
using ArturRios.Util.Tests.Setup;

namespace ArturRios.Util.Tests.IO;

public class FileReaderTests
{
    [Fact]
    public void GivenValidPath_WhenRead_ThenReturnFileContent()
    {
        var path = FileTestHelper.CreateTempFile("Hello World");

        try
        {
            var result = FileReader.Read(path);

            Assert.Equal("Hello World", result);
        }

        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenNullOrWhitespacePath_WhenRead_ThenThrowArgumentException(string? path)
    {
        var exception = Assert.Throws<ArgumentException>(() => FileReader.Read(path!));

        Assert.Equal("path", exception.ParamName);
        Assert.Equal("Path cannot be null or whitespace (Parameter 'path')", exception.Message);
    }

    [Fact]
    public void GivenMissingFile_WhenRead_ThenThrowFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception = Assert.Throws<FileNotFoundException>(() => FileReader.Read(missingPath));

        Assert.Equal($"The file at path '{missingPath}' does not exist", exception.Message);
    }

    [Fact]
    public void GivenValidPath_WhenReadLines_ThenReturnAllLines()
    {
        var path = FileTestHelper.CreateTempFile("line1\nline2\nline3");

        try
        {
            var lines = FileReader.ReadLines(path);

            Assert.Equal(["line1", "line2", "line3"], lines);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenNullOrWhitespacePath_WhenReadLines_ThenThrowArgumentException(string? path)
    {
        var exception = Assert.Throws<ArgumentException>(() => FileReader.ReadLines(path!));

        Assert.Equal("path", exception.ParamName);
        Assert.Equal("Path cannot be null or whitespace (Parameter 'path')", exception.Message);
    }

    [Fact]
    public void GivenMissingFile_WhenReadLines_ThenThrowFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception = Assert.Throws<FileNotFoundException>(() => FileReader.ReadLines(missingPath));

        Assert.Equal($"The file at path '{missingPath}' does not exist", exception.Message);
    }

    [Fact]
    public void GivenValidCsv_WhenReadAsDictionary_ThenReturnDictionary()
    {
        var content = string.Join('\n', "Key1,Key2,Key3", "A,B,C", "D,E,F");

        var path = FileTestHelper.CreateTempFile(content);

        try
        {
            var dict = FileReader.ReadAsDictionary(path, ',');

            Assert.Equal(3, dict.Count);

            Assert.Equal(["A", "D"], dict["Key1"]);
            Assert.Equal(["B", "E"], dict["Key2"]);
            Assert.Equal(["C", "F"], dict["Key3"]);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void GivenMissingValuesInLaterLines_WhenReadAsDictionary_ThenHandleMissingValues()
    {
        var content = string.Join('\n', "Key1,Key2,Key3", "A,B,C", "Z");

        var path = FileTestHelper.CreateTempFile(content);

        try
        {
            var dict = FileReader.ReadAsDictionary(path, ',');

            Assert.Equal(["A", "Z"], dict["Key1"]);
            Assert.Equal(["B", ""], dict["Key2"]);
            Assert.Equal(["C", ""], dict["Key3"]);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void GivenExtraValuesBeyondHeaders_WhenReadAsDictionary_ThenIgnoreExtras()
    {
        var content = string.Join('\n', "Key1,Key2", "1,2,3", "4,5,6");

        var path = FileTestHelper.CreateTempFile(content);

        try
        {
            var dictionary = FileReader.ReadAsDictionary(path, ',');

            Assert.Equal(["1", "4"], dictionary["Key1"]);
            Assert.Equal(["2", "5"], dictionary["Key2"]);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void GivenFileWithOnlyHeader_WhenReadAsDictionary_ThenThrowException()
    {
        var path = FileTestHelper.CreateTempFile("Header1,Header2,Header3");

        try
        {
            var exception = Assert.Throws<InvalidOperationException>(() => FileReader.ReadAsDictionary(path, ','));

            Assert.Equal("File must have at least a header and one data line", exception.Message);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenNullOrWhitespacePath_WhenReadAsDictionary_ThenThrowArgumentException(string? path)
    {
        var exception = Assert.Throws<ArgumentException>(() => FileReader.ReadAsDictionary(path!, ','));

        Assert.Equal("path", exception.ParamName);
    }

    [Fact]
    public void GivenMissingFile_WhenReadAsDictionary_ThenThrowFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception = Assert.Throws<FileNotFoundException>(() => FileReader.ReadAsDictionary(missingPath, ','));

        Assert.Equal($"The file at path '{missingPath}' does not exist", exception.Message);
    }


    [Fact]
    public void GivenValidJsonFile_WhenReadAndDeserialize_ThenReturnTypedObject()
    {
        var obj = new Person { Name = "Alice", Age = 30, Home = new Address { Street = "Main", Number = 100 } };
        var json = JsonSerializer.Serialize(obj);
        var path = FileTestHelper.CreateTempFile(json);

        try
        {
            var result = FileReader.ReadAndDeserialize<Person>(path);

            Assert.NotNull(result);
            Assert.Equal("Alice", result.Name);
            Assert.Equal(30, result.Age);
            Assert.Equal("Main", result.Home.Street);
            Assert.Equal(100, result.Home.Number);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenNullOrWhitespacePath_WhenReadAndDeserialize_ThenThrowArgumentException(string? path)
    {
        var exception = Assert.Throws<ArgumentException>(() => FileReader.ReadAndDeserialize<Person>(path!));

        Assert.Equal("path", exception.ParamName);
    }

    [Fact]
    public void GivenMissingFile_WhenReadAndDeserialize_ThenThrowFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception = Assert.Throws<FileNotFoundException>(() => FileReader.ReadAndDeserialize<Person>(missingPath));

        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public void GivenInvalidJsonFile_WhenReadAndDeserialize_ThenThrowJsonException()
    {
        var path = FileTestHelper.CreateTempFile("{ invalid json }");

        try
        {
            Assert.Throws<JsonException>(() => FileReader.ReadAndDeserialize<Person>(path));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
