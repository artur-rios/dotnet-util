using System.Net.Mime;
using ArturRios.Util.Http;
using ArturRios.Util.Tests.Setup;

namespace ArturRios.Util.Tests.Http;

public class HttpExtensionsTests
{
    [Fact]
    public async Task Should_CreateValidJsonStringContent()
    {
        var person = new Person { Name = "Alice", Age = 30, Home = new Address { Street = "Main", Number = 100 } };

        var content = person.ToJsonStringContent();

        Assert.NotNull(content);
        Assert.Equal(MediaTypeNames.Application.Json, content.Headers.ContentType!.MediaType);

        var json = await content.ReadAsStringAsync();

        Assert.Contains("\"Name\":\"Alice\"", json);
        Assert.Contains("\"Age\":30", json);
        Assert.Contains("\"Home\":", json);
    }
}
