namespace ArturRios.Util.Console;

/// <summary>
/// Helpers for writing formatted output to the system console.
/// </summary>
public static class CustomConsole
{
    /// <summary>
    /// Writes a line composed of a single character repeated a given number of times.
    /// </summary>
    /// <param name="c">The character to repeat. Defaults to '-'.</param>
    /// <param name="quantity">How many times to repeat <paramref name="c"/>. Defaults to 100.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quantity"/> is negative.</exception>
    public static void WriteCharLine(char c = '-', int quantity = 100) => System.Console.WriteLine(new string(c, quantity));
}
