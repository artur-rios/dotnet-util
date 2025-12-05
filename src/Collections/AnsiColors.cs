namespace ArturRios.Util.Collections;

/// <summary>
/// Provides ANSI escape code color constants for console output formatting.
/// </summary>
/// <remarks>
/// These strings can be prefixed to console text to apply color and should usually be followed by an ANSI reset ("\x1b[0m") after usage.
/// </remarks>
public static class AnsiColors
{
    /// <summary>ANSI color code for dark gray (bright black) foreground text.</summary>
    public const string DarkGray = "\x1b[90m";

    /// <summary>ANSI color code for cyan foreground text.</summary>
    public const string Cyan = "\x1b[36m";

    /// <summary>ANSI color code for white foreground text.</summary>
    public const string White = "\x1b[37m";

    /// <summary>ANSI color code for yellow foreground text.</summary>
    public const string Yellow = "\x1b[33m";

    /// <summary>ANSI color code for red foreground text.</summary>
    public const string Red = "\x1b[31m";

    /// <summary>ANSI color code for magenta foreground text.</summary>
    public const string Magenta = "\x1b[35m";

    /// <summary>ANSI color code for bright red (bold red) foreground text.</summary>
    public const string BrightRed = "\x1b[31;1m";

    /// <summary>ANSI color code for green foreground text.</summary>
    public const string Green = "\x1b[32m";
}
