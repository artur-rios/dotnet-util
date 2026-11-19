using System.Text.Json;
using ArturRios.Util.IO;
using ArturRios.Util.Tests.Setup;

namespace ArturRios.Util.Tests.IO;

[Trait("Category", "Functional")]
public class FileReaderTests
{
    [Fact]
    public void GivenValidPath_WhenRead_ThenReturnFileContent()
    {
        using var file = new TempFile("Hello World");

        var result = FileReader.Read(file);

        Assert.Equal("Hello World", result);
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
        using var file = new TempFile("line1\nline2\nline3");

        var lines = FileReader.ReadLines(file);

        Assert.Equal(["line1", "line2", "line3"], lines);
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

        using var file = new TempFile(content);

        var dict = FileReader.ReadAsDictionary(file, ',');

        Assert.Equal(3, dict.Count);

        Assert.Equal(["A", "D"], dict["Key1"]);
        Assert.Equal(["B", "E"], dict["Key2"]);
        Assert.Equal(["C", "F"], dict["Key3"]);
    }

    [Fact]
    public void GivenMissingValuesInLaterLines_WhenReadAsDictionary_ThenHandleMissingValues()
    {
        var content = string.Join('\n', "Key1,Key2,Key3", "A,B,C", "Z");

        using var file = new TempFile(content);

        var dict = FileReader.ReadAsDictionary(file, ',');

        Assert.Equal(["A", "Z"], dict["Key1"]);
        Assert.Equal(["B", ""], dict["Key2"]);
        Assert.Equal(["C", ""], dict["Key3"]);
    }

    [Fact]
    public void GivenExtraValuesBeyondHeaders_WhenReadAsDictionary_ThenIgnoreExtras()
    {
        var content = string.Join('\n', "Key1,Key2", "1,2,3", "4,5,6");

        using var file = new TempFile(content);

        var dictionary = FileReader.ReadAsDictionary(file, ',');

        Assert.Equal(["1", "4"], dictionary["Key1"]);
        Assert.Equal(["2", "5"], dictionary["Key2"]);
    }

    [Fact]
    public void GivenFileWithOnlyHeader_WhenReadAsDictionary_ThenThrowException()
    {
        using var file = new TempFile("Header1,Header2,Header3");

        var exception = Assert.Throws<InvalidOperationException>(() => FileReader.ReadAsDictionary(file, ','));

        Assert.Equal("File must have at least a header and one data line", exception.Message);
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
        using var file = new TempFile(json);

        var result = FileReader.ReadAndDeserialize<Person>(file);

        Assert.NotNull(result);
        Assert.Equal("Alice", result.Name);
        Assert.Equal(30, result.Age);
        Assert.Equal("Main", result.Home.Street);
        Assert.Equal(100, result.Home.Number);
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
        using var file = new TempFile("{ invalid json }");

        Assert.Throws<JsonException>(() => FileReader.ReadAndDeserialize<Person>(file));
    }

    [Fact]
    public void GivenDuplicateHeaders_WhenReadAsDictionary_ThenThrowInsteadOfDroppingAColumn()
    {
        // A silent overwrite used to return a dictionary with fewer entries than the file has columns.
        using var file = TempFile.WithLines("name,value,name", "a,1,b");

        var exception = Assert.Throws<InvalidOperationException>(() => FileReader.ReadAsDictionary(file, ','));

        Assert.Contains("duplicate", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("'name'", exception.Message);
    }

    [Fact]
    public void GivenHeadersDifferingOnlyByCase_WhenReadAsDictionary_ThenTreatThemAsDistinct()
    {
        using var file = TempFile.WithLines("Name,name", "a,b");

        var result = FileReader.ReadAsDictionary(file, ',');

        Assert.Equal(["a"], result["Name"]);
        Assert.Equal(["b"], result["name"]);
    }

    [Fact]
    public void GivenEmptyHeaderName_WhenReadAsDictionary_ThenKeepItAsAnEmptyKey()
    {
        using var file = TempFile.WithLines("name,,age", "a,b,30");

        var result = FileReader.ReadAsDictionary(file, ',');

        Assert.Equal(["b"], result[string.Empty]);
    }

    [Fact]
    public void GivenQuotedFieldContainingTheSeparator_WhenReadAsDictionary_ThenSplitItAnyway()
    {
        // Documented limitation: this is a plain split, not an RFC 4180 parser.
        using var file = TempFile.WithLines("a,b", "\"x,y\",z");

        var result = FileReader.ReadAsDictionary(file, ',');

        Assert.Equal(["\"x"], result["a"]);
        Assert.Equal(["y\""], result["b"]);
    }

    [Fact]
    public void GivenEmptyFile_WhenReadAsDictionary_ThenThrowInvalidOperationException()
    {
        using var file = new TempFile(string.Empty);

        Assert.Throws<InvalidOperationException>(() => FileReader.ReadAsDictionary(file, ','));
    }

    [Fact]
    public void GivenSeparatorAbsentFromTheFile_WhenReadAsDictionary_ThenReturnASingleColumn()
    {
        using var file = TempFile.WithLines("header", "first", "second");

        var result = FileReader.ReadAsDictionary(file, ',');

        Assert.Equal(["first", "second"], result["header"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenEmptyOrWhitespaceJsonFile_WhenReadAndDeserialize_ThenThrowJsonException(string content)
    {
        // The documentation used to promise null here; the serializer has always thrown.
        using var file = new TempFile(content);

        Assert.Throws<JsonException>(() => FileReader.ReadAndDeserialize<Person>(file));
    }

    [Fact]
    public void GivenJsonNullLiteral_WhenReadAndDeserialize_ThenReturnNull()
    {
        using var file = new TempFile("null");

        Assert.Null(FileReader.ReadAndDeserialize<Person>(file));
    }

    [Fact]
    public void GivenJsonOfTheWrongShape_WhenReadAndDeserialize_ThenThrowJsonException()
    {
        using var file = new TempFile("[1, 2, 3]");

        Assert.Throws<JsonException>(() => FileReader.ReadAndDeserialize<Person>(file));
    }

    [Fact]
    public void GivenEmptyFile_WhenRead_ThenReturnEmptyString()
    {
        using var file = new TempFile(string.Empty);

        Assert.Equal(string.Empty, FileReader.Read(file));
    }

    [Fact]
    public void GivenEmptyFile_WhenReadLines_ThenReturnEmptyArray()
    {
        using var file = new TempFile(string.Empty);

        Assert.Empty(FileReader.ReadLines(file));
    }

    [Fact]
    public void GivenCamelCaseJsonFile_WhenReadAndDeserialize_ThenPropertiesBindRegardlessOfCase()
    {
        using var file = new TempFile("{\"name\":\"Alice\",\"age\":30,\"home\":{\"street\":\"Main\",\"number\":100}}");

        var result = FileReader.ReadAndDeserialize<Person>(file);

        Assert.NotNull(result);
        Assert.Equal("Alice", result.Name);
        Assert.Equal(30, result.Age);
        Assert.Equal("Main", result.Home.Street);
        Assert.Equal(100, result.Home.Number);
    }

    [Theory]
    [InlineData(';')]
    [InlineData('\t')]
    [InlineData('|')]
    public void GivenAnotherSeparator_WhenReadAsDictionary_ThenSplitOnIt(char separator)
    {
        using var file = TempFile.WithLines($"name{separator}age", $"Ana{separator}30", $"Bia,Silva{separator}41");

        var result = FileReader.ReadAsDictionary(file, separator);

        Assert.Equal(["Ana", "Bia,Silva"], result["name"]);
        Assert.Equal(["30", "41"], result["age"]);
    }

    [Fact]
    public void GivenWindowsAndUnixLineEndings_WhenReadLines_ThenNeitherIsKeptInTheLines()
    {
        using var file = new TempFile("first\r\nsecond\nthird");

        Assert.Equal(["first", "second", "third"], FileReader.ReadLines(file));
    }

    [Fact]
    public void GivenNonAsciiUtf8Content_WhenRead_ThenDecodeIt()
    {
        using var file = new TempFile("a\u00e7\u00e3o \u4f60\u597d");

        Assert.Equal("a\u00e7\u00e3o \u4f60\u597d", FileReader.Read(file));
    }

    [Fact]
    public void GivenMissingFile_WhenRead_ThenTheExceptionNamesTheFile()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing");

        Assert.Equal(missingPath, Assert.Throws<FileNotFoundException>(() => FileReader.Read(missingPath)).FileName);
    }

    [Fact]
    public void GivenJsonArray_WhenReadAndDeserialize_ThenReturnTheCollection()
    {
        using var file = new TempFile("[{\"Name\":\"Ana\",\"Age\":30},{\"Name\":\"Bia\",\"Age\":41}]");

        var result = FileReader.ReadAndDeserialize<List<Person>>(file);

        Assert.Equal(["Ana", "Bia"], result!.Select(person => person.Name));
    }
}
