using System.Text.Json;
using ArturRios.Util.IO;
using ArturRios.Util.Tests.Setup;

namespace ArturRios.Util.Tests.IO;

public class FileReaderAsyncTests
{
    [Fact]
    public async Task Should_ReturnFileContent()
    {
        var path = FileTestHelper.CreateTempFile("Hello World");

        try
        {
            var result = await FileReaderAsync.ReadAsync(path);

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
    public async Task Should_ThrowArgumentException_WhenPathIsNullOrWhitespace(string? path)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => FileReaderAsync.ReadAsync(path!));

        Assert.Equal("path", exception.ParamName);
        Assert.Equal("Path cannot be null or whitespace (Parameter 'path')", exception.Message);
    }

    [Fact]
    public async Task Should_ThrowFileNotFoundException_WhenFileDoesNotExist()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => FileReaderAsync.ReadAsync(missingPath));

        Assert.Equal($"The file at path '{missingPath}' does not exist", exception.Message);
    }

    [Fact]
    public async Task Should_ReturnAllLines()
    {
        var path = FileTestHelper.CreateTempFile("line1\nline2\nline3");

        try
        {
            var lines = await FileReaderAsync.ReadLinesAsync(path);

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
    public async Task Should_ThrowArgumentExceptionOnReadLines_WhenPathIsNullOrWhitespace(string? path)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => FileReaderAsync.ReadLinesAsync(path!));

        Assert.Equal("path", exception.ParamName);
        Assert.Equal("Path cannot be null or whitespace (Parameter 'path')", exception.Message);
    }

    [Fact]
    public async Task Should_ThrowFileNotFoundExceptionOnReadLines_WhenFileDoesNotExist()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception =
            await Assert.ThrowsAsync<FileNotFoundException>(() => FileReaderAsync.ReadLinesAsync(missingPath));

        Assert.Equal($"The file at path '{missingPath}' does not exist", exception.Message);
    }

    [Fact]
    public async Task Should_ReadAsDictionary()
    {
        var content = string.Join('\n', "Key1,Key2,Key3", "A,B,C", "D,E,F");

        var path = FileTestHelper.CreateTempFile(content);

        try
        {
            var dict = await FileReaderAsync.ReadAsDictionaryAsync(path, ',');

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
    public async Task Should_HandleMissingValuesInLaterLines()
    {
        var content = string.Join('\n', "Key1,Key2,Key3", "A,B,C", "Z");

        var path = FileTestHelper.CreateTempFile(content);

        try
        {
            var dict = await FileReaderAsync.ReadAsDictionaryAsync(path, ',');

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
    public async Task Should_IgnoreExtraValuesBeyondHeaders()
    {
        var content = string.Join('\n', "Key1,Key2", "1,2,3", "4,5,6");

        var path = FileTestHelper.CreateTempFile(content);

        try
        {
            var dictionary = await FileReaderAsync.ReadAsDictionaryAsync(path, ',');

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
    public async Task Should_ThrowExceptionWhenFileHasLessThanTwoLines()
    {
        var path = FileTestHelper.CreateTempFile("Header1,Header2,Header3");

        try
        {
            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    FileReaderAsync.ReadAsDictionaryAsync(path, ','));

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
    public async Task Should_ThrowArgumentExceptionOnReadAsDictionary_WhenPathIsNullOrWhitespace(string? path)
    {
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(() => FileReaderAsync.ReadAsDictionaryAsync(path!, ','));

        Assert.Equal("path", exception.ParamName);
    }

    [Fact]
    public async Task Should_ThrowFileNotFoundExceptionOnReadAsDictionary_WhenFileDoesNotExist()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception =
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                FileReaderAsync.ReadAsDictionaryAsync(missingPath, ','));

        Assert.Equal($"The file at path '{missingPath}' does not exist", exception.Message);
    }

    [Fact]
    public async Task Should_ReadAndDeserialize_ReturnTypedObject()
    {
        var obj = new Person("Alice", 30);
        var json = JsonSerializer.Serialize(obj);
        var path = FileTestHelper.CreateTempFile(json);

        try
        {
            var result = await FileReaderAsync.ReadAndDeserializeAsync<Person>(path);

            Assert.NotNull(result);
            Assert.Equal("Alice", result.Name);
            Assert.Equal(30, result.Age);
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
    public async Task Should_ReadAndDeserialize_ThrowArgumentException_WhenPathIsNullOrWhitespace(string? path)
    {
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(() => FileReaderAsync.ReadAndDeserializeAsync<Person>(path!));

        Assert.Equal("path", exception.ParamName);
    }

    [Fact]
    public async Task Should_ReadAndDeserialize_ThrowFileNotFoundException_WhenFileDoesNotExist()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception =
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                FileReaderAsync.ReadAndDeserializeAsync<Person>(missingPath));

        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public async Task Should_ReadAndDeserialize_ThrowJsonException_ForInvalidJson()
    {
        var path = FileTestHelper.CreateTempFile("{ invalid json }");

        try
        {
            await Assert.ThrowsAsync<JsonException>(() => FileReaderAsync.ReadAndDeserializeAsync<Person>(path));
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
