using System.Text.Json;

namespace ArturRios.Util.IO;

/// <summary>
/// Provides asynchronous helper methods for reading and deserializing file contents.
/// </summary>
/// <remarks>
/// All methods validate the provided path and throw <see cref="ArgumentException"/> for null/whitespace and <see cref="FileNotFoundException"/> when the file does not exist.
/// </remarks>
public static class FileReaderAsync
{
    /// <summary>
    /// Asynchronously reads the entire contents of a text file.
    /// </summary>
    /// <param name="path">Absolute or relative path to the file.</param>
    /// <returns>File content as a string.</returns>
    /// <exception cref="ArgumentException">Path is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    public static async Task<string> ReadAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or whitespace", nameof(path));
        }

        return File.Exists(path)
            ? await File.ReadAllTextAsync(path)
            : throw new FileNotFoundException($"The file at path '{path}' does not exist", path);
    }

    /// <summary>
    /// Asynchronously reads a delimited text file (e.g. CSV) mapping each header to its column values.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <param name="separator">Delimiter separating fields (e.g. <c>','</c>).</param>
    /// <returns>A dictionary keyed by column header mapping to arrays of its values.</returns>
    /// <exception cref="ArgumentException">Path is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    /// <exception cref="InvalidOperationException">File must contain a header and at least one data line.</exception>
    public static async Task<Dictionary<string, string[]>> ReadAsDictionaryAsync(string path, char separator)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or whitespace", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The file at path '{path}' does not exist", path);
        }

        var lines = await File.ReadAllLinesAsync(path);

        if (lines.Length < 2)
        {
            throw new InvalidOperationException("File must have at least a header and one data line");
        }

        var headers = lines[0].Split(separator);
        var columnLists = new List<string>[headers.Length];
        for (int i = 0; i < headers.Length; i++)
        {
            columnLists[i] = new List<string>();
        }

        for (var row = 1; row < lines.Length; row++)
        {
            var values = lines[row].Split(separator);

            for (var col = 0; col < headers.Length; col++)
            {
                var value = col < values.Length ? values[col] : string.Empty;
                columnLists[col].Add(value);
            }
        }

        var dict = new Dictionary<string, string[]>(headers.Length);
        for (int i = 0; i < headers.Length; i++)
        {
            dict[headers[i]] = columnLists[i].ToArray();
        }

        return dict;
    }

    /// <summary>
    /// Asynchronously reads all lines of a text file into an array.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <returns>Array of lines.</returns>
    /// <exception cref="ArgumentException">Path is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    public static async Task<string[]> ReadLinesAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or whitespace", nameof(path));
        }

        return File.Exists(path)
            ? await File.ReadAllLinesAsync(path)
            : throw new FileNotFoundException($"The file at path '{path}' does not exist", path);
    }

    /// <summary>
    /// Asynchronously reads a JSON file and deserializes its content into the specified type.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <returns>Deserialized object or <c>null</c> if content is empty or invalid JSON.</returns>
    /// <exception cref="ArgumentException">Path is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    public static async Task<T?> ReadAndDeserializeAsync<T>(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or whitespace", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The file at path '{path}' does not exist", path);
        }

        var content = await File.ReadAllTextAsync(path);

        return JsonSerializer.Deserialize<T>(content);
    }
}
