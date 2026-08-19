namespace ArturRios.Util.IO;

/// <summary>
/// Turns the lines of a delimited text file into a column-per-header dictionary.
/// </summary>
/// <remarks>
/// Shared by <see cref="FileReader"/> and <see cref="FileReaderAsync"/> so the two cannot drift apart.
/// </remarks>
internal static class DelimitedText
{
    /// <summary>
    /// Maps each header in the first line to the values found beneath it.
    /// </summary>
    /// <param name="lines">The lines of the file, header first.</param>
    /// <param name="separator">Delimiter separating fields.</param>
    /// <exception cref="InvalidOperationException">
    /// Fewer than two lines were supplied, or a header name occurs more than once.
    /// </exception>
    internal static Dictionary<string, string[]> ToDictionary(string[] lines, char separator)
    {
        if (lines.Length < 2)
        {
            throw new InvalidOperationException("File must have at least a header and one data line");
        }

        var headers = lines[0].Split(separator);

        // Later columns would otherwise overwrite earlier ones, quietly returning fewer columns than the
        // file has.
        var duplicates = headers
            .GroupBy(header => header, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => $"'{group.Key}'")
            .ToArray();

        if (duplicates.Length > 0)
        {
            throw new InvalidOperationException(
                $"Header row contains duplicate column names: {string.Join(", ", duplicates)}");
        }

        var columnLists = new List<string>[headers.Length];

        for (var i = 0; i < headers.Length; i++)
        {
            columnLists[i] = new List<string>(lines.Length - 1);
        }

        for (var row = 1; row < lines.Length; row++)
        {
            var values = lines[row].Split(separator);

            for (var col = 0; col < headers.Length; col++)
            {
                columnLists[col].Add(col < values.Length ? values[col] : string.Empty);
            }
        }

        var dictionary = new Dictionary<string, string[]>(headers.Length);

        for (var i = 0; i < headers.Length; i++)
        {
            dictionary[headers[i]] = [.. columnLists[i]];
        }

        return dictionary;
    }
}
