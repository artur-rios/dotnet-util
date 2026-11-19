using System.Text;
using System.Text.Json;
using ArturRios.Util.IO;
using ArturRios.Util.Tests.Setup;

namespace ArturRios.Util.Tests.IO;

[Trait("Category", "Functional")]
public class FileReaderAsyncTests
{
    [Fact]
    public async Task GivenValidPath_WhenReadAsync_ThenReturnFileContent()
    {
        using var file = new TempFile("Hello World");

        var result = await FileReaderAsync.ReadAsync(file, TestContext.Current.CancellationToken);

        Assert.Equal("Hello World", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GivenNullOrWhitespacePath_WhenReadAsync_ThenThrowArgumentException(string? path)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => FileReaderAsync.ReadAsync(path!, TestContext.Current.CancellationToken));

        Assert.Equal("path", exception.ParamName);
        Assert.Equal("Path cannot be null or whitespace (Parameter 'path')", exception.Message);
    }

    [Fact]
    public async Task GivenMissingFile_WhenReadAsync_ThenThrowFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => FileReaderAsync.ReadAsync(missingPath, TestContext.Current.CancellationToken));

        Assert.Equal($"The file at path '{missingPath}' does not exist", exception.Message);
    }

    [Fact]
    public async Task GivenValidPath_WhenReadLinesAsync_ThenReturnAllLines()
    {
        using var file = new TempFile("line1\nline2\nline3");

        var lines = await FileReaderAsync.ReadLinesAsync(file, TestContext.Current.CancellationToken);

        Assert.Equal(["line1", "line2", "line3"], lines);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GivenNullOrWhitespacePath_WhenReadLinesAsync_ThenThrowArgumentException(string? path)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => FileReaderAsync.ReadLinesAsync(path!, TestContext.Current.CancellationToken));

        Assert.Equal("path", exception.ParamName);
        Assert.Equal("Path cannot be null or whitespace (Parameter 'path')", exception.Message);
    }

    [Fact]
    public async Task GivenMissingFile_WhenReadLinesAsync_ThenThrowFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception =
            await Assert.ThrowsAsync<FileNotFoundException>(() => FileReaderAsync.ReadLinesAsync(missingPath, TestContext.Current.CancellationToken));

        Assert.Equal($"The file at path '{missingPath}' does not exist", exception.Message);
    }

    [Fact]
    public async Task GivenValidCsv_WhenReadAsDictionaryAsync_ThenReturnDictionary()
    {
        var content = string.Join('\n', "Key1,Key2,Key3", "A,B,C", "D,E,F");

        using var file = new TempFile(content);

        var dict = await FileReaderAsync.ReadAsDictionaryAsync(file, ',', TestContext.Current.CancellationToken);

        Assert.Equal(3, dict.Count);

        Assert.Equal(["A", "D"], dict["Key1"]);
        Assert.Equal(["B", "E"], dict["Key2"]);
        Assert.Equal(["C", "F"], dict["Key3"]);
    }

    [Fact]
    public async Task GivenMissingValuesInLaterLines_WhenReadAsDictionaryAsync_ThenHandleMissingValues()
    {
        var content = string.Join('\n', "Key1,Key2,Key3", "A,B,C", "Z");

        using var file = new TempFile(content);

        var dict = await FileReaderAsync.ReadAsDictionaryAsync(file, ',', TestContext.Current.CancellationToken);

        Assert.Equal(["A", "Z"], dict["Key1"]);
        Assert.Equal(["B", ""], dict["Key2"]);
        Assert.Equal(["C", ""], dict["Key3"]);
    }

    [Fact]
    public async Task GivenExtraValuesBeyondHeaders_WhenReadAsDictionaryAsync_ThenIgnoreExtras()
    {
        var content = string.Join('\n', "Key1,Key2", "1,2,3", "4,5,6");

        using var file = new TempFile(content);

        var dictionary = await FileReaderAsync.ReadAsDictionaryAsync(file, ',', TestContext.Current.CancellationToken);

        Assert.Equal(["1", "4"], dictionary["Key1"]);
        Assert.Equal(["2", "5"], dictionary["Key2"]);
    }

    [Fact]
    public async Task GivenFileWithOnlyHeader_WhenReadAsDictionaryAsync_ThenThrowException()
    {
        using var file = new TempFile("Header1,Header2,Header3");

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                FileReaderAsync.ReadAsDictionaryAsync(file, ',', TestContext.Current.CancellationToken));

        Assert.Equal("File must have at least a header and one data line", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GivenNullOrWhitespacePath_WhenReadAsDictionaryAsync_ThenThrowArgumentException(string? path)
    {
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(() => FileReaderAsync.ReadAsDictionaryAsync(path!, ',', TestContext.Current.CancellationToken));

        Assert.Equal("path", exception.ParamName);
    }

    [Fact]
    public async Task GivenMissingFile_WhenReadAsDictionaryAsync_ThenThrowFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception =
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                FileReaderAsync.ReadAsDictionaryAsync(missingPath, ',', TestContext.Current.CancellationToken));

        Assert.Equal($"The file at path '{missingPath}' does not exist", exception.Message);
    }

    [Fact]
    public async Task GivenValidJsonFile_WhenReadAndDeserializeAsync_ThenReturnTypedObject()
    {
        var obj = new Person { Name = "Alice", Age = 30, Home = new Address { Street = "Main", Number = 100 } };
        var json = JsonSerializer.Serialize(obj);
        using var file = new TempFile(json);

        var result = await FileReaderAsync.ReadAndDeserializeAsync<Person>(file, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Alice", result.Name);
        Assert.Equal(30, result.Age);
        Assert.Equal("Main", result.Home.Street);
        Assert.Equal(100, result.Home.Number);
    }

    public static TheoryData<string> JsonEncodings => ["utf-8", "utf-8-bom", "utf-16le", "utf-16be", "utf-32le"];

    private static Encoding EncodingNamed(string name) => name switch
    {
        "utf-8" => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        "utf-8-bom" => new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
        "utf-16le" => new UnicodeEncoding(bigEndian: false, byteOrderMark: true),
        "utf-16be" => new UnicodeEncoding(bigEndian: true, byteOrderMark: true),
        "utf-32le" => new UTF32Encoding(bigEndian: false, byteOrderMark: true),
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
    };

    [Theory]
    [MemberData(nameof(JsonEncodings))]
    public async Task GivenAJsonFileWithAByteOrderMark_WhenReadAndDeserializeAsync_ThenItReadsAsTheSynchronousReaderDoes(
        string encoding)
    {
        using var file = new TempFile(string.Empty);
        File.WriteAllText(file, """{ "Name": "Zoë", "Age": 30 }""", EncodingNamed(encoding));

        var result = await FileReaderAsync.ReadAndDeserializeAsync<Person>(file, TestContext.Current.CancellationToken);
        var synchronous = FileReader.ReadAndDeserialize<Person>(file);

        Assert.NotNull(result);
        Assert.Equal("Zoë", result.Name);
        Assert.Equal(30, result.Age);
        Assert.Equal(synchronous?.Name, result.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GivenNullOrWhitespacePath_WhenReadAndDeserializeAsync_ThenThrowArgumentException(string? path)
    {
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(() => FileReaderAsync.ReadAndDeserializeAsync<Person>(path!, TestContext.Current.CancellationToken));

        Assert.Equal("path", exception.ParamName);
    }

    [Fact]
    public async Task GivenMissingFile_WhenReadAndDeserializeAsync_ThenThrowFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception =
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                FileReaderAsync.ReadAndDeserializeAsync<Person>(missingPath, TestContext.Current.CancellationToken));

        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public async Task GivenInvalidJsonFile_WhenReadAndDeserializeAsync_ThenThrowJsonException()
    {
        using var file = new TempFile("{ invalid json }");

        await Assert.ThrowsAsync<JsonException>(() => FileReaderAsync.ReadAndDeserializeAsync<Person>(file, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenDuplicateHeaders_WhenReadAsDictionaryAsync_ThenThrowInsteadOfDroppingAColumn()
    {
        // A silent overwrite used to return a dictionary with fewer entries than the file has columns.
        using var file = TempFile.WithLines("name,value,name", "a,1,b");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => FileReaderAsync.ReadAsDictionaryAsync(file, ',', TestContext.Current.CancellationToken));

        Assert.Contains("duplicate", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("'name'", exception.Message);
    }

    [Fact]
    public async Task GivenHeadersDifferingOnlyByCase_WhenReadAsDictionaryAsync_ThenTreatThemAsDistinct()
    {
        using var file = TempFile.WithLines("Name,name", "a,b");

        var result = await FileReaderAsync.ReadAsDictionaryAsync(file, ',', TestContext.Current.CancellationToken);

        Assert.Equal(["a"], result["Name"]);
        Assert.Equal(["b"], result["name"]);
    }

    [Fact]
    public async Task GivenEmptyHeaderName_WhenReadAsDictionaryAsync_ThenKeepItAsAnEmptyKey()
    {
        using var file = TempFile.WithLines("name,,age", "a,b,30");

        var result = await FileReaderAsync.ReadAsDictionaryAsync(file, ',', TestContext.Current.CancellationToken);

        Assert.Equal(["b"], result[string.Empty]);
    }

    [Fact]
    public async Task GivenQuotedFieldContainingTheSeparator_WhenReadAsDictionaryAsync_ThenSplitItAnyway()
    {
        // Documented limitation: this is a plain split, not an RFC 4180 parser.
        using var file = TempFile.WithLines("a,b", "\"x,y\",z");

        var result = await FileReaderAsync.ReadAsDictionaryAsync(file, ',', TestContext.Current.CancellationToken);

        Assert.Equal(["\"x"], result["a"]);
        Assert.Equal(["y\""], result["b"]);
    }

    [Fact]
    public async Task GivenEmptyFile_WhenReadAsDictionaryAsync_ThenThrowInvalidOperationException()
    {
        using var file = new TempFile(string.Empty);

        await Assert.ThrowsAsync<InvalidOperationException>(() => FileReaderAsync.ReadAsDictionaryAsync(file, ',', TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenSeparatorAbsentFromTheFile_WhenReadAsDictionaryAsync_ThenReturnASingleColumn()
    {
        using var file = TempFile.WithLines("header", "first", "second");

        var result = await FileReaderAsync.ReadAsDictionaryAsync(file, ',', TestContext.Current.CancellationToken);

        Assert.Equal(["first", "second"], result["header"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GivenEmptyOrWhitespaceJsonFile_WhenReadAndDeserializeAsync_ThenThrowJsonException(string content)
    {
        // The documentation used to promise null here; the serializer has always thrown.
        using var file = new TempFile(content);

        await Assert.ThrowsAsync<JsonException>(() => FileReaderAsync.ReadAndDeserializeAsync<Person>(file, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenJsonNullLiteral_WhenReadAndDeserializeAsync_ThenReturnNull()
    {
        using var file = new TempFile("null");

        Assert.Null(await FileReaderAsync.ReadAndDeserializeAsync<Person>(file, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenJsonOfTheWrongShape_WhenReadAndDeserializeAsync_ThenThrowJsonException()
    {
        using var file = new TempFile("[1, 2, 3]");

        await Assert.ThrowsAsync<JsonException>(() => FileReaderAsync.ReadAndDeserializeAsync<Person>(file, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenEmptyFile_WhenReadAsync_ThenReturnEmptyString()
    {
        using var file = new TempFile(string.Empty);

        Assert.Equal(string.Empty, await FileReaderAsync.ReadAsync(file, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenEmptyFile_WhenReadLinesAsync_ThenReturnEmptyArray()
    {
        using var file = new TempFile(string.Empty);

        Assert.Empty(await FileReaderAsync.ReadLinesAsync(file, TestContext.Current.CancellationToken));
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

        var asynchronous = await FileReaderAsync.ReadAsDictionaryAsync(file, ',', TestContext.Current.CancellationToken);
        var synchronous = FileReader.ReadAsDictionary(file, ',');

        Assert.Equal(synchronous, asynchronous);
    }

    [Fact]
    public async Task GivenCamelCaseJsonFile_WhenReadAndDeserializeAsync_ThenPropertiesBindRegardlessOfCase()
    {
        using var file = new TempFile("{\"name\":\"Alice\",\"age\":30,\"home\":{\"street\":\"Main\",\"number\":100}}");

        var result = await FileReaderAsync.ReadAndDeserializeAsync<Person>(file, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Alice", result.Name);
        Assert.Equal(30, result.Age);
        Assert.Equal("Main", result.Home.Street);
        Assert.Equal(100, result.Home.Number);
    }

    [Fact]
    public async Task GivenAnotherSeparator_WhenReadAsDictionaryAsync_ThenSplitOnIt()
    {
        using var file = TempFile.WithLines("name;age", "Ana;30", "Bia,Silva;41");

        var result = await FileReaderAsync.ReadAsDictionaryAsync(file, ';', TestContext.Current.CancellationToken);

        Assert.Equal(["Ana", "Bia,Silva"], result["name"]);
        Assert.Equal(["30", "41"], result["age"]);
    }

    [Fact]
    public async Task GivenWindowsAndUnixLineEndings_WhenReadLinesAsync_ThenNeitherIsKeptInTheLines()
    {
        using var file = new TempFile("first\r\nsecond\nthird");

        Assert.Equal(["first", "second", "third"], await FileReaderAsync.ReadLinesAsync(file, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenMissingFile_WhenReadAsync_ThenTheExceptionNamesTheFile()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => FileReaderAsync.ReadAsync(missingPath, TestContext.Current.CancellationToken));

        Assert.Equal(missingPath, exception.FileName);
    }
}
