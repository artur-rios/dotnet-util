namespace ArturRios.Util.Random;

/// <summary>
/// Options controlling random string generation produced by <see cref="CustomRandom.Text"/>.
/// </summary>
public class RandomStringOptions
{
    /// <summary>
    /// Desired length of the generated string. Default is 10.
    /// </summary>
    public int Length { get; set; } = 10;
    /// <summary>
    /// Whether to include lowercase letters. Default is <c>true</c>.
    /// </summary>
    public bool IncludeLowercase { get; set; } = true;
    /// <summary>
    /// Whether to include uppercase letters. Default is <c>true</c>.
    /// </summary>
    public bool IncludeUppercase { get; set; } = true;
    /// <summary>
    /// Whether to include digits. Default is <c>true</c>.
    /// </summary>
    public bool IncludeDigits { get; set; } = true;
    /// <summary>
    /// Whether to include special characters. Default is <c>true</c>.
    /// </summary>
    public bool IncludeSpecialCharacters { get; set; } = true;
}
