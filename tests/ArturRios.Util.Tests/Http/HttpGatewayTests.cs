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
        {
            // A real handler observes the token; the stub has to as well for cancellation to be testable.
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(responder(request));
        }
    }

    private static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
        => new(new StubHandler(responder)) { BaseAddress = new Uri("https://example.test/") };

    public record SampleDto(string Message, int Value);

    [Fact]
    public async Task GivenGetRequest_WhenReceivingJsonResponse_ThenReturnDeserializedBodyAndStatus()
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
    public async Task GivenPostRequest_WhenSendingJsonPayload_ThenDeserializeResponseAndCaptureRequestBody()
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
    public async Task GivenPatchRequest_WhenPayloadIsNull_ThenHandleNullPayload()
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
    public async Task GivenPutRequest_WhenSendingPayload_ThenCaptureRequestBodyAndReturnResponse()
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
    public async Task GivenDeleteRequest_WhenReceivingJsonResponse_ThenReturnStatusAndBody()
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

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GivenNonSuccessStatus_WhenGetAsync_ThenReportItWithoutThrowing(HttpStatusCode statusCode)
    {
        var client = CreateClient(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent("{\"error\":\"nope\"}", Encoding.UTF8, "application/json")
        });

        var output = await new HttpGateway(client).GetAsync<SampleDto>("api/test");

        Assert.Equal(statusCode, output.StatusCode);
        Assert.False(output.IsSuccess);
    }

    [Fact]
    public async Task GivenResponseOfTheWrongShape_WhenGetAsync_ThenReturnNullBodyInsteadOfThrowing()
    {
        // The gateway used to let a JSON shape mismatch propagate out of the call.
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[1, 2, 3]", Encoding.UTF8, "application/json")
        });

        var output = await new HttpGateway(client).GetAsync<SampleDto>("api/test");

        Assert.Null(output.Body);
        Assert.Equal("[1, 2, 3]", output.RawBody);
    }

    [Fact]
    public async Task GivenHtmlErrorPage_WhenGetAsync_ThenReturnNullBodyInsteadOfThrowing()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("<html><body>502</body></html>", Encoding.UTF8, "text/html")
        });

        var output = await new HttpGateway(client).GetAsync<SampleDto>("api/test");

        Assert.Null(output.Body);
        Assert.False(output.IsSuccess);
    }

    [Fact]
    public async Task GivenResponseHeaders_WhenGetAsync_ThenPropagateThem()
    {
        var client = CreateClient(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };

            response.Headers.Add("X-Trace-Id", "abc123");

            return response;
        });

        var output = await new HttpGateway(client).GetAsync<SampleDto>("api/test");

        Assert.Equal("abc123", output.Headers.GetValues("X-Trace-Id").Single());
        Assert.Equal("application/json", output.ContentHeaders.ContentType!.MediaType);
    }

    [Fact]
    public async Task GivenCancelledToken_WhenGetAsync_ThenThrowOperationCanceledException()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });

        using var cancellation = new CancellationTokenSource();

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new HttpGateway(client).GetAsync<SampleDto>("api/test", cancellation.Token));
    }

    [Fact]
    public async Task GivenCancelledToken_WhenPostAsync_ThenThrowOperationCanceledException()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });

        using var cancellation = new CancellationTokenSource();

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new HttpGateway(client).PostAsync<SampleDto>("api/test", new { Value = 1 }, cancellation.Token));
    }

    [Fact]
    public async Task GivenTransportFailure_WhenGetAsync_ThenLetTheExceptionPropagate()
    {
        var client = new HttpClient(new ThrowingHandler()) { BaseAddress = new Uri("https://example.test/") };

        await Assert.ThrowsAsync<HttpRequestException>(() => new HttpGateway(client).GetAsync<SampleDto>("api/test"));
    }

    [Fact]
    public async Task GivenSuccessfulCall_WhenItReturns_ThenTheResponseMessageIsDisposed()
    {
        HttpResponseMessage? captured = null;

        var client = CreateClient(_ => captured = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });

        var output = await new HttpGateway(client).GetAsync<SampleDto>("api/test");

        Assert.NotNull(captured);

        // Reading a disposed response's content throws; the buffered output is unaffected.
        await Assert.ThrowsAsync<ObjectDisposedException>(() => captured!.Content.ReadAsStringAsync());
        Assert.Equal("{}", output.RawBody);
    }

    [Fact]
    public async Task GivenCamelCaseResponse_WhenGetAsync_ThenBindToPascalCaseMembers()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"message\":\"hello\",\"value\":42}", Encoding.UTF8, "application/json")
        });

        var output = await new HttpGateway(client).GetAsync<SampleDto>("api/test");

        Assert.NotNull(output.Body);
        Assert.Equal("hello", output.Body!.Message);
        Assert.Equal(42, output.Body.Value);
    }

    [Fact]
    public void GivenNullClient_WhenConstructed_ThenThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new HttpGateway(null!));
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
            => throw new HttpRequestException("connection refused");
    }
}
