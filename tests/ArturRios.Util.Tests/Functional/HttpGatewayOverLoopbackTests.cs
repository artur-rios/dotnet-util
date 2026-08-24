using System.Net;
using System.Text;
using ArturRios.Util.Http;

namespace ArturRios.Util.Tests.Functional;

/// <summary>
/// Drives <see cref="HttpGateway"/> against a real HTTP server on the loopback interface, so the request
/// really crosses a socket and the response really comes back over the wire. The unit tests cover the same
/// surface against a stubbed message handler.
/// </summary>
[Trait("Category", "Functional")]
public sealed class HttpGatewayOverLoopbackTests : IAsyncLifetime
{
    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _shutdown = new();
    private readonly List<RecordedRequest> _received = [];
    private Task _serverLoop = Task.CompletedTask;
    private HttpClient _client = null!;

    private Func<RecordedRequest, (HttpStatusCode Status, string Body, string ContentType)> _responder =
        _ => (HttpStatusCode.OK, "{}", "application/json");

    public Task InitializeAsync()
    {
        var port = FreeTcpPort();

        _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        _listener.Start();

        _serverLoop = Task.Run(ServeAsync);
        _client = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}/") };

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _shutdown.CancelAsync();

        _listener.Close();
        _client.Dispose();

        try
        {
            await _serverLoop;
        }
        catch (Exception)
        {
            // The listener is torn down under the loop on purpose; whatever it throws on the way out is
            // not a test result.
        }

        _shutdown.Dispose();
    }

    [Fact]
    public async Task GivenARunningServer_WhenSendingAGet_ThenTheJsonBodyComesBackDeserialised()
    {
        _responder = _ => (HttpStatusCode.OK, """{"Message":"hello","Value":42}""", "application/json");

        var output = await new HttpGateway(_client).GetAsync<SampleDto>("api/items");

        Assert.True(output.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, output.StatusCode);
        Assert.NotNull(output.Body);
        Assert.Equal("hello", output.Body!.Message);
        Assert.Equal(42, output.Body.Value);
        Assert.Equal("""{"Message":"hello","Value":42}""", output.RawBody);
    }

    [Fact]
    public async Task GivenARunningServer_WhenPostingAPayload_ThenTheServerReceivesTheSerialisedJson()
    {
        _responder = _ => (HttpStatusCode.Created, """{"Message":"created","Value":1}""", "application/json");

        var output = await new HttpGateway(_client).PostAsync<SampleDto>("api/items", new SampleDto("new", 7));

        Assert.Equal(HttpStatusCode.Created, output.StatusCode);
        Assert.True(output.IsSuccess);

        var request = Assert.Single(_received);

        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/items", request.Path);
        Assert.Contains("application/json", request.ContentType);
        Assert.Equal("""{"Message":"new","Value":7}""", request.Body);
    }

    [Fact]
    public async Task GivenAServerAnsweringWithCamelCase_WhenSendingAGet_ThenPropertiesStillBind()
    {
        _responder = _ => (HttpStatusCode.OK, """{"message":"lower","value":3}""", "application/json");

        var output = await new HttpGateway(_client).GetAsync<SampleDto>("api/items");

        Assert.NotNull(output.Body);
        Assert.Equal("lower", output.Body!.Message);
        Assert.Equal(3, output.Body.Value);
    }

    [Fact]
    public async Task GivenAServerAnsweringWithHtml_WhenSendingAGet_ThenTheRawBodyIsKeptAndTheBodyIsNull()
    {
        _responder = _ => (HttpStatusCode.InternalServerError, "<html><body>boom</body></html>", "text/html");

        var output = await new HttpGateway(_client).GetAsync<SampleDto>("api/items");

        Assert.False(output.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, output.StatusCode);
        Assert.Null(output.Body);
        Assert.Equal("<html><body>boom</body></html>", output.RawBody);
    }

    [Fact]
    public async Task GivenAServerAnsweringWithNoContent_WhenSendingADelete_ThenTheOutputIsSuccessfulWithNoBody()
    {
        _responder = _ => (HttpStatusCode.NoContent, string.Empty, "application/json");

        var output = await new HttpGateway(_client).DeleteAsync<SampleDto>("api/items/1");

        Assert.True(output.IsSuccess);
        Assert.Equal(HttpStatusCode.NoContent, output.StatusCode);
        Assert.Null(output.Body);
        Assert.Equal(string.Empty, output.RawBody);
    }

    [Fact]
    public async Task GivenARunningServer_WhenPuttingAndPatching_ThenBothVerbsReachTheServer()
    {
        _responder = _ => (HttpStatusCode.OK, """{"Message":"ok","Value":0}""", "application/json");

        var gateway = new HttpGateway(_client);

        await gateway.PutAsync<SampleDto>("api/items/1", new SampleDto("put", 1));
        await gateway.PatchAsync<SampleDto>("api/items/1", new SampleDto("patch", 2));

        Assert.Equal(new[] { "PUT", "PATCH" }, _received.Select(x => x.Method));
    }

    [Fact]
    public async Task GivenANullPayload_WhenPosting_ThenNoRequestBodyIsSent()
    {
        _responder = _ => (HttpStatusCode.OK, """{"Message":"ok","Value":0}""", "application/json");

        await new HttpGateway(_client).PostAsync<SampleDto>("api/items");

        var request = Assert.Single(_received);

        Assert.Equal(string.Empty, request.Body);
    }

    private async Task ServeAsync()
    {
        while (!_shutdown.IsCancellationRequested)
        {
            HttpListenerContext context;

            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (HttpListenerException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);

            var recorded = new RecordedRequest(
                context.Request.HttpMethod,
                context.Request.Url?.AbsolutePath ?? string.Empty,
                context.Request.ContentType ?? string.Empty,
                await reader.ReadToEndAsync());

            lock (_received)
            {
                _received.Add(recorded);
            }

            var (status, body, contentType) = _responder(recorded);
            var buffer = Encoding.UTF8.GetBytes(body);

            context.Response.StatusCode = (int)status;
            context.Response.ContentType = contentType;
            context.Response.ContentLength64 = buffer.Length;

            await context.Response.OutputStream.WriteAsync(buffer);

            context.Response.Close();
        }
    }

    private static int FreeTcpPort()
    {
        using var socket = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);

        socket.Start();

        var port = ((IPEndPoint)socket.LocalEndpoint).Port;

        socket.Stop();

        return port;
    }

    private sealed record RecordedRequest(string Method, string Path, string ContentType, string Body);

    private sealed record SampleDto(string Message, int Value);
}
