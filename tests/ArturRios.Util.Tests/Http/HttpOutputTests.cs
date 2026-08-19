using System.Net;
using System.Text;
using ArturRios.Util.Http;

namespace ArturRios.Util.Tests.Http;

public class HttpOutputTests
{
    [Fact]
    public async Task GivenJsonResponse_WhenReadContentAsync_ThenDeserializeIntoBody()
    {
        const string json = "{\"name\":\"john\",\"age\":30}";

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var output = new HttpOutput<Person?>(response);

        await output.ReadContentAsync();

        Assert.Equal(HttpStatusCode.OK, output.StatusCode);
        Assert.NotNull(output.Body);
        Assert.Equal("john", output.Body!.Name);
        Assert.Equal(30, output.Body.Age);
    }

    [Fact]
    public async Task GivenInvalidJsonResponse_WhenReadContentAsync_ThenReturnDefault()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "text/plain")
        };

        var output = new HttpOutput<Person?>(response);

        await output.ReadContentAsync();

        Assert.Null(output.Body);
    }

    [Fact]
    public async Task GivenJsonArrayWhereAnObjectIsExpected_WhenReadContentAsync_ThenReturnDefaultInsteadOfThrowing()
    {
        // A shape mismatch raises a different exception type from a syntax error. Catching only the
        // syntax error let this escape all the way out of HttpGateway.
        var output = OutputFor<Person?>(HttpStatusCode.OK, "[1, 2, 3]");

        await output.ReadContentAsync();

        Assert.Null(output.Body);
        Assert.Equal("[1, 2, 3]", output.RawBody);
    }

    [Fact]
    public async Task GivenJsonWithAMismatchedFieldType_WhenReadContentAsync_ThenReturnDefaultInsteadOfThrowing()
    {
        var output = OutputFor<Person?>(HttpStatusCode.OK, "{\"name\":\"john\",\"age\":\"not a number\"}");

        await output.ReadContentAsync();

        Assert.Null(output.Body);
    }

    [Fact]
    public async Task GivenHtmlErrorPage_WhenReadContentAsync_ThenReturnDefaultAndKeepTheRawBody()
    {
        var output = OutputFor<Person?>(HttpStatusCode.BadGateway, "<html><body>502 Bad Gateway</body></html>");

        await output.ReadContentAsync();

        Assert.Null(output.Body);
        Assert.Equal("<html><body>502 Bad Gateway</body></html>", output.RawBody);
        Assert.False(output.IsSuccess);
    }

    [Fact]
    public async Task GivenEmptyBody_WhenReadContentAsync_ThenReturnDefault()
    {
        var output = OutputFor<Person?>(HttpStatusCode.NoContent, string.Empty);

        await output.ReadContentAsync();

        Assert.Null(output.Body);
        Assert.Equal(string.Empty, output.RawBody);
        Assert.True(output.IsSuccess);
    }

    [Fact]
    public async Task GivenValueTypeBodyAndEmptyResponse_WhenReadContentAsync_ThenReturnDefaultInsteadOfThrowing()
    {
        var output = OutputFor<int>(HttpStatusCode.OK, string.Empty);

        await output.ReadContentAsync();

        Assert.Equal(0, output.Body);
    }

    [Fact]
    public async Task GivenCamelCaseJson_WhenReadContentAsync_ThenBindToPascalCaseMembers()
    {
        var output = OutputFor<Person?>(HttpStatusCode.OK, "{\"name\":\"john\",\"age\":30}");

        await output.ReadContentAsync();

        Assert.NotNull(output.Body);
        Assert.Equal("john", output.Body!.Name);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.NoContent, true)]
    [InlineData(HttpStatusCode.MovedPermanently, false)]
    [InlineData(HttpStatusCode.NotFound, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void GivenStatusCode_WhenIsSuccess_ThenReportWhetherItIs2xx(HttpStatusCode statusCode, bool expected)
    {
        Assert.Equal(expected, OutputFor<Person?>(statusCode, "{}").IsSuccess);
    }

    // Cancellation of the body read is covered by HttpGatewayTests: an in-memory StringContent completes
    // without ever observing the token, so only a real response stream can exercise it.

    [Fact]
    public async Task GivenResponseHeaders_WhenConstructed_ThenExposeBothResponseAndContentHeaders()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };

        response.Headers.Add("X-Trace-Id", "abc123");

        var output = new HttpOutput<Person?>(response);

        await output.ReadContentAsync();

        Assert.Equal("abc123", output.Headers.GetValues("X-Trace-Id").Single());
        Assert.Equal("application/json", output.ContentHeaders.ContentType!.MediaType);
    }

    private static HttpOutput<T> OutputFor<T>(HttpStatusCode statusCode, string body) =>
        new(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });

    private record Person(string Name, int Age);
}
