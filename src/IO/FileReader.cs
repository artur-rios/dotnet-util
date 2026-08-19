using System.Text.Json;

namespace ArturRios.Util.IO;

/// <summary>
/// Provides synchronous helper methods for reading and deserializing file contents.
/// </summary>
/// <remarks>
/// All methods validate the provided path and throw <see cref="ArgumentException"/> for null/whitespace and <see cref="FileNotFoundException"/> when the file does not exist.
/// See <see cref="FileReaderAsync"/> for the asynchronous equivalents, which behave identically.
/// </remarks>
public static class FileReader
{
    /// <summary>
    /// Reads the entire contents of a text file.
    /// </summary>
    /// <param name="path">Absolute or relative path to the file.</param>
    /// <returns>File content as a string.</returns>
    /// <exception cref="ArgumentException">Path is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    public static string Read(string path)
    {
        ValidatePath(path);

        return File.ReadAllText(path);
    }

    /// <summary>
    /// Reads a delimited text file (e.g. CSV) mapping each header to its column values.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <param name="separator">Delimiter separating fields (e.g. <c>','</c>).</param>
    /// <returns>A dictionary keyed by column header mapping to arrays of its values.</returns>
    /// <exception cref="ArgumentException">Path is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    /// <exception cref="InvalidOperationException">
    /// The file has fewer than two lines, or the header row repeats a name, which would silently drop a
    /// column from the result.
    /// </exception>
    /// <remarks>
    /// This is a plain split on <paramref name="separator"/>, not an RFC 4180 parser: quoting is not
    /// interpreted, so a quoted field containing the separator or a line break is split like any other.
    /// Use a dedicated CSV library when the data can contain either. Rows shorter than the header are
    /// padded with empty strings, and values beyond the last header are discarded.
    /// </remarks>
    public static Dictionary<string, string[]> ReadAsDictionary(string path, char separator)
    {
        ValidatePath(path);

        return DelimitedText.ToDictionary(File.ReadAllLines(path), separator);
    }

    /// <summary>
    /// Reads all lines of a text file into an array.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <returns>Array of lines.</returns>
    /// <exception cref="ArgumentException">Path is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    public static string[] ReadLines(string path)
    {
        ValidatePath(path);

        return File.ReadAllLines(path);
    }

    /// <summary>
    /// Reads a JSON file and deserializes its content into the specified type.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <returns>
    /// The deserialized object, or <c>null</c> when the file contains the JSON literal <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentException">Path is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    /// <exception cref="JsonException">
    /// The file is empty, holds only whitespace, or does not contain JSON matching
    /// <typeparamref name="T"/>. An unreadable file is reported rather than silently turned into
    /// <c>null</c>, which would be indistinguishable from a genuine null payload.
    /// </exception>
    public static T? ReadAndDeserialize<T>(string path)
    {
        ValidatePath(path);

        return JsonSerializer.Deserialize<T>(File.ReadAllText(path));
    }

    /// <summary>
    /// Rejects an unusable path before any I/O is attempted.
    /// </summary>
    /// <exception cref="ArgumentException">Path is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    internal static void ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or whitespace", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The file at path '{path}' does not exist", path);
        }
    }
}
