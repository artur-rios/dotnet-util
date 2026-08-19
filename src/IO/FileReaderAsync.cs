using System.Text.Json;

namespace ArturRios.Util.IO;

/// <summary>
/// Provides asynchronous helper methods for reading and deserializing file contents.
/// </summary>
/// <remarks>
/// All methods validate the provided path and throw <see cref="ArgumentException"/> for null/whitespace and <see cref="FileNotFoundException"/> when the file does not exist.
/// Behavior matches <see cref="FileReader"/> exactly; only the I/O is asynchronous.
/// </remarks>
public static class FileReaderAsync
{
    /// <summary>
    /// Asynchronously reads the entire contents of a text file.
    /// </summary>
    /// <param name="path">Absolute or relative path to the file.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>File content as a string.</returns>
    /// <exception cref="ArgumentException">Path is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
    public static async Task<string> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        FileReader.ValidatePath(path);

        return await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously reads a delimited text file (e.g. CSV) mapping each header to its column values.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <param name="separator">Delimiter separating fields (e.g. <c>','</c>).</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>A dictionary keyed by column header mapping to arrays of its values.</returns>
    /// <exception cref="ArgumentException">Path is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    /// <exception cref="InvalidOperationException">
    /// The file has fewer than two lines, or the header row repeats a name, which would silently drop a
    /// column from the result.
    /// </exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
    /// <remarks>
    /// This is a plain split on <paramref name="separator"/>, not an RFC 4180 parser: quoting is not
    /// interpreted, so a quoted field containing the separator or a line break is split like any other.
    /// Use a dedicated CSV library when the data can contain either. Rows shorter than the header are
    /// padded with empty strings, and values beyond the last header are discarded.
    /// </remarks>
    public static async Task<Dictionary<string, string[]>> ReadAsDictionaryAsync(
        string path,
        char separator,
        CancellationToken cancellationToken = default)
    {
        FileReader.ValidatePath(path);

        var lines = await File.ReadAllLinesAsync(path, cancellationToken).ConfigureAwait(false);

        return DelimitedText.ToDictionary(lines, separator);
    }

    /// <summary>
    /// Asynchronously reads all lines of a text file into an array.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>Array of lines.</returns>
    /// <exception cref="ArgumentException">Path is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
    public static async Task<string[]> ReadLinesAsync(string path, CancellationToken cancellationToken = default)
    {
        FileReader.ValidatePath(path);

        return await File.ReadAllLinesAsync(path, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously reads a JSON file and deserializes its content into the specified type.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <param name="cancellationToken">Cancels the read and the deserialization.</param>
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
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
    public static async Task<T?> ReadAndDeserializeAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        FileReader.ValidatePath(path);

        await using var stream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
