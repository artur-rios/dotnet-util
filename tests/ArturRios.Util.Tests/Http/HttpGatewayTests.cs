using System.Net;
using System.Text;
using ArturRios.Util.Http;

namespace ArturRios.Util.Tests.Http;

public class HttpGatewayTests
{
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }

    private static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
        => new(new StubHandler(responder)) { BaseAddress = new Uri("https://example.test/") };

    public record SampleDto(string Message, int Value);

    [Fact]
    public async Task Should_ReturnDeserializedBodyAndStatus()
    {
        const string bodyJson = "{\"Message\":\"hello\",\"Value\":42}";

        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
        });

        var gateway = new HttpGateway(client);

        var output = await gateway.GetAsync<SampleDto>("api/test");

        Assert.Equal(HttpStatusCode.OK, output.StatusCode);
        Assert.NotNull(output.Body);
        Assert.Equal("hello", output.Body!.Message);
        Assert.Equal(42, output.Body.Value);
    }

    [Fact]
    public async Task Should_SendJsonPayloadAndDeserializeResponse()
    {
        string? capturedRequestBody = null;

        const string responseJson = "{\"Message\":\"created\",\"Value\":1}";

        var client = CreateClient(req =>
        {
            capturedRequestBody = req.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
        });

        var gateway = new HttpGateway(client);

        var output = await gateway.PostAsync<SampleDto>("api/items", new { Name = "abc" });

        Assert.Equal(HttpStatusCode.Created, output.StatusCode);
        Assert.NotNull(output.Body);
        Assert.Equal("created", output.Body!.Message);
        Assert.Equal(1, output.Body.Value);
        Assert.Contains("\"Name\":\"abc\"", capturedRequestBody);
    }

    [Fact]
    public async Task Should_HandleNullPayload()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NoContent)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
        });

        var gateway = new HttpGateway(client);

        var output = await gateway.PatchAsync<object>("api/patch");

        Assert.Equal(HttpStatusCode.NoContent, output.StatusCode);
        Assert.Null(output.Body);
    }

    [Fact]
    public async Task Should_SendPayload()
    {
        string? capturedRequestBody = null;

        var client = CreateClient(req =>
        {
            capturedRequestBody = req.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Message\":\"ok\",\"Value\":7}", Encoding.UTF8, "application/json")
            };
        });

        var gateway = new HttpGateway(client);

        var output = await gateway.PutAsync<SampleDto>("api/put", new { X = 7 });

        Assert.Equal(HttpStatusCode.OK, output.StatusCode);
        Assert.NotNull(output.Body);
        Assert.Contains("\"X\":7", capturedRequestBody);
    }

    [Fact]
    public async Task Should_ReturnStatusAndBody()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"Message\":\"deleted\",\"Value\":0}", Encoding.UTF8, "application/json")
        });

        var gateway = new HttpGateway(client);

        var output = await gateway.DeleteAsync<SampleDto>("api/items/1");

        Assert.Equal(HttpStatusCode.OK, output.StatusCode);
        Assert.Equal("deleted", output.Body!.Message);
        Assert.Equal(0, output.Body.Value);
    }
}
