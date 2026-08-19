using System.Text.Json;
using ArturRios.Util.IO;
using ArturRios.Util.Tests.Setup;

namespace ArturRios.Util.Tests.IO;

public class FileReaderAsyncTests
{
    [Fact]
    public async Task GivenValidPath_WhenReadAsync_ThenReturnFileContent()
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
    public async Task GivenNullOrWhitespacePath_WhenReadAsync_ThenThrowArgumentException(string? path)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => FileReaderAsync.ReadAsync(path!));

        Assert.Equal("path", exception.ParamName);
        Assert.Equal("Path cannot be null or whitespace (Parameter 'path')", exception.Message);
    }

    [Fact]
    public async Task GivenMissingFile_WhenReadAsync_ThenThrowFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => FileReaderAsync.ReadAsync(missingPath));

        Assert.Equal($"The file at path '{missingPath}' does not exist", exception.Message);
    }

    [Fact]
    public async Task GivenValidPath_WhenReadLinesAsync_ThenReturnAllLines()
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
    public async Task GivenNullOrWhitespacePath_WhenReadLinesAsync_ThenThrowArgumentException(string? path)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => FileReaderAsync.ReadLinesAsync(path!));

        Assert.Equal("path", exception.ParamName);
        Assert.Equal("Path cannot be null or whitespace (Parameter 'path')", exception.Message);
    }

    [Fact]
    public async Task GivenMissingFile_WhenReadLinesAsync_ThenThrowFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception =
            await Assert.ThrowsAsync<FileNotFoundException>(() => FileReaderAsync.ReadLinesAsync(missingPath));

        Assert.Equal($"The file at path '{missingPath}' does not exist", exception.Message);
    }

    [Fact]
    public async Task GivenValidCsv_WhenReadAsDictionaryAsync_ThenReturnDictionary()
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
    public async Task GivenMissingValuesInLaterLines_WhenReadAsDictionaryAsync_ThenHandleMissingValues()
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
    public async Task GivenExtraValuesBeyondHeaders_WhenReadAsDictionaryAsync_ThenIgnoreExtras()
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
    public async Task GivenFileWithOnlyHeader_WhenReadAsDictionaryAsync_ThenThrowException()
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
    public async Task GivenNullOrWhitespacePath_WhenReadAsDictionaryAsync_ThenThrowArgumentException(string? path)
    {
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(() => FileReaderAsync.ReadAsDictionaryAsync(path!, ','));

        Assert.Equal("path", exception.ParamName);
    }

    [Fact]
    public async Task GivenMissingFile_WhenReadAsDictionaryAsync_ThenThrowFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception =
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                FileReaderAsync.ReadAsDictionaryAsync(missingPath, ','));

        Assert.Equal($"The file at path '{missingPath}' does not exist", exception.Message);
    }

    [Fact]
    public async Task GivenValidJsonFile_WhenReadAndDeserializeAsync_ThenReturnTypedObject()
    {
        var obj = new Person { Name = "Alice", Age = 30, Home = new Address { Street = "Main", Number = 100 } };
        var json = JsonSerializer.Serialize(obj);
        var path = FileTestHelper.CreateTempFile(json);

        try
        {
            var result = await FileReaderAsync.ReadAndDeserializeAsync<Person>(path);

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
    public async Task GivenNullOrWhitespacePath_WhenReadAndDeserializeAsync_ThenThrowArgumentException(string? path)
    {
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(() => FileReaderAsync.ReadAndDeserializeAsync<Person>(path!));

        Assert.Equal("path", exception.ParamName);
    }

    [Fact]
    public async Task GivenMissingFile_WhenReadAndDeserializeAsync_ThenThrowFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception =
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                FileReaderAsync.ReadAndDeserializeAsync<Person>(missingPath));

        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public async Task GivenInvalidJsonFile_WhenReadAndDeserializeAsync_ThenThrowJsonException()
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

    [Fact]
    public async Task GivenDuplicateHeaders_WhenReadAsDictionaryAsync_ThenThrowInsteadOfDroppingAColumn()
    {
        // A silent overwrite used to return a dictionary with fewer entries than the file has columns.
        using var file = TempFile.WithLines("name,value,name", "a,1,b");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => FileReaderAsync.ReadAsDictionaryAsync(file, ','));

        Assert.Contains("duplicate", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("'name'", exception.Message);
    }

    [Fact]
    public async Task GivenHeadersDifferingOnlyByCase_WhenReadAsDictionaryAsync_ThenTreatThemAsDistinct()
    {
        using var file = TempFile.WithLines("Name,name", "a,b");

        var result = await FileReaderAsync.ReadAsDictionaryAsync(file, ',');

        Assert.Equal(["a"], result["Name"]);
        Assert.Equal(["b"], result["name"]);
    }

    [Fact]
    public async Task GivenEmptyHeaderName_WhenReadAsDictionaryAsync_ThenKeepItAsAnEmptyKey()
    {
        using var file = TempFile.WithLines("name,,age", "a,b,30");

        var result = await FileReaderAsync.ReadAsDictionaryAsync(file, ',');

        Assert.Equal(["b"], result[string.Empty]);
    }

    [Fact]
    public async Task GivenQuotedFieldContainingTheSeparator_WhenReadAsDictionaryAsync_ThenSplitItAnyway()
    {
        // Documented limitation: this is a plain split, not an RFC 4180 parser.
        using var file = TempFile.WithLines("a,b", "\"x,y\",z");

        var result = await FileReaderAsync.ReadAsDictionaryAsync(file, ',');

        Assert.Equal(["\"x"], result["a"]);
        Assert.Equal(["y\""], result["b"]);
    }

    [Fact]
    public async Task GivenEmptyFile_WhenReadAsDictionaryAsync_ThenThrowInvalidOperationException()
    {
        using var file = new TempFile(string.Empty);

        await Assert.ThrowsAsync<InvalidOperationException>(() => FileReaderAsync.ReadAsDictionaryAsync(file, ','));
    }

    [Fact]
    public async Task GivenSeparatorAbsentFromTheFile_WhenReadAsDictionaryAsync_ThenReturnASingleColumn()
    {
        using var file = TempFile.WithLines("header", "first", "second");

        var result = await FileReaderAsync.ReadAsDictionaryAsync(file, ',');

        Assert.Equal(["first", "second"], result["header"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GivenEmptyOrWhitespaceJsonFile_WhenReadAndDeserializeAsync_ThenThrowJsonException(string content)
    {
        // The documentation used to promise null here; the serializer has always thrown.
        using var file = new TempFile(content);

        await Assert.ThrowsAsync<JsonException>(() => FileReaderAsync.ReadAndDeserializeAsync<Person>(file));
    }

    [Fact]
    public async Task GivenJsonNullLiteral_WhenReadAndDeserializeAsync_ThenReturnNull()
    {
        using var file = new TempFile("null");

        Assert.Null(await FileReaderAsync.ReadAndDeserializeAsync<Person>(file));
    }

    [Fact]
    public async Task GivenJsonOfTheWrongShape_WhenReadAndDeserializeAsync_ThenThrowJsonException()
    {
        using var file = new TempFile("[1, 2, 3]");

        await Assert.ThrowsAsync<JsonException>(() => FileReaderAsync.ReadAndDeserializeAsync<Person>(file));
    }

    [Fact]
    public async Task GivenEmptyFile_WhenReadAsync_ThenReturnEmptyString()
    {
        using var file = new TempFile(string.Empty);

        Assert.Equal(string.Empty, await FileReaderAsync.ReadAsync(file));
    }

    [Fact]
    public async Task GivenEmptyFile_WhenReadLinesAsync_ThenReturnEmptyArray()
    {
        using var file = new TempFile(string.Empty);

        Assert.Empty(await FileReaderAsync.ReadLinesAsync(file));
    }

    [Fact]
    public async Task GivenCancelledToken_WhenReadingAsync_ThenThrowOperationCanceledException()
    {
        using var file = TempFile.WithLines("a,b", "1,2");
        using var cancellation = new CancellationTokenSource();

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => FileReaderAsync.ReadAsync(file, cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => FileReaderAsync.ReadLinesAsync(file, cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => FileReaderAsync.ReadAsDictionaryAsync(file, ',', cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => FileReaderAsync.ReadAndDeserializeAsync<Person>(file, cancellation.Token));
    }

    [Fact]
    public async Task GivenTheSameInput_WhenReadAsDictionaryAsync_ThenMatchTheSynchronousReader()
    {
        using var file = TempFile.WithLines("a,b,c", "1,2,3", "4,5");

        var asynchronous = await FileReaderAsync.ReadAsDictionaryAsync(file, ',');
        var synchronous = FileReader.ReadAsDictionary(file, ',');

        Assert.Equal(synchronous, asynchronous);
    }
}
