using System.Net.Mime;
using ArturRios.Util.Http;
using ArturRios.Util.Tests.Setup;

namespace ArturRios.Util.Tests.Http;

public class HttpExtensionsTests
{
    [Fact]
    public async Task GivenObject_WhenToJsonStringContent_ThenCreateValidJsonStringContent()
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

    [Fact]
    public async Task GivenBoxedObjectReference_WhenToJsonStringContent_ThenSerializeTheRuntimeType()
    {
        object payload = new Person { Name = "Alice", Age = 30, Home = new Address { Street = "Main", Number = 1 } };

        var json = await payload.ToJsonStringContent().ReadAsStringAsync();

        Assert.Contains("\"Name\":\"Alice\"", json);
    }

    [Fact]
    public async Task GivenAnonymousObject_WhenToJsonStringContent_ThenSerializeItsMembers()
    {
        var json = await new { Value = 1, Nested = new { Flag = true } }.ToJsonStringContent().ReadAsStringAsync();

        Assert.Contains("\"Value\":1", json);
        Assert.Contains("\"Flag\":true", json);
    }

    [Fact]
    public async Task GivenCollection_WhenToJsonStringContent_ThenSerializeAsAJsonArray()
    {
        var json = await new[] { 1, 2, 3 }.ToJsonStringContent().ReadAsStringAsync();

        Assert.Equal("[1,2,3]", json);
    }

    [Fact]
    public async Task GivenNullReference_WhenToJsonStringContent_ThenSerializeTheJsonNullLiteral()
    {
        Person? payload = null;

        var json = await payload.ToJsonStringContent().ReadAsStringAsync();

        Assert.Equal("null", json);
    }

    [Fact]
    public void GivenAnyPayload_WhenToJsonStringContent_ThenUseUtf8AndTheJsonMediaType()
    {
        var headers = new { Value = 1 }.ToJsonStringContent().Headers;

        Assert.Equal(MediaTypeNames.Application.Json, headers.ContentType!.MediaType);
        Assert.Equal("utf-8", headers.ContentType.CharSet);
    }
}
