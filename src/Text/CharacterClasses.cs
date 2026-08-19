namespace ArturRios.Util.Text;

/// <summary>
/// The ASCII character classes a piece of text can contain.
/// </summary>
/// <remarks>
/// The classes mirror the constants in <see cref="Collections.Characters"/>, so a value classified here
/// can be compared directly against the pools used to generate random text.
/// </remarks>
[Flags]
public enum CharacterClasses
{
    /// <summary>No class is present.</summary>
    None = 0,

    /// <summary>At least one digit 0-9 is present.</summary>
    Digit = 1,

    /// <summary>At least one lowercase letter a-z is present.</summary>
    Lower = 2,

    /// <summary>At least one uppercase letter A-Z is present.</summary>
    Upper = 4,

    /// <summary>At least one <see cref="Collections.Characters.Special"/> character is present.</summary>
    Special = 8
}
