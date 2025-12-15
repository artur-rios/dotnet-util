namespace ArturRios.Util.Tests.Setup;

public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public Address Home { get; set; } = new();
}
