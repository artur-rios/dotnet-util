using System.Net;
using System.Text;
using ArturRios.Util.Http;

namespace ArturRios.Util.Tests.Http;

public class HttpOutputTests
{
    [Fact]
    public async Task Should_DeserializeJsonIntoBody()
    {
        const string json = "{\"name\":\"john\",\"age\":30}";

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var output = new HttpOutput<Person?>(response);

        await output.ReadContent();

        Assert.Equal(HttpStatusCode.OK, output.StatusCode);
        Assert.NotNull(output.Body);
        Assert.Equal("john", output.Body!.Name);
        Assert.Equal(30, output.Body.Age);
    }

    [Fact]
    public async Task Should_HandleInvalidJsonByReturningDefault()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "text/plain")
        };

        var output = new HttpOutput<Person?>(response);

        await output.ReadContent();

        Assert.Null(output.Body);
    }

    private record Person(string Name, int Age);
}
