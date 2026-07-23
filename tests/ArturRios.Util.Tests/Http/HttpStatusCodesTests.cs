using ArturRios.Util.Http;

namespace ArturRios.Util.Tests.Http;

public class HttpStatusCodesTests
{
    [Fact]
    public void GivenStatusCodes_WhenAccessingSuccessGroup_ThenContains2xxCodes()
    {
        Assert.Contains(HttpStatusCodes.Ok, HttpStatusCodes.Success);
        Assert.Contains(HttpStatusCodes.Created, HttpStatusCodes.Success);
        Assert.Contains(HttpStatusCodes.Accepted, HttpStatusCodes.Success);
        Assert.Contains(HttpStatusCodes.NoContent, HttpStatusCodes.Success);
    }

    [Fact]
    public void GivenStatusCodes_WhenAccessingRedirectionGroup_ThenContains3xxCodes()
    {
        Assert.Contains(HttpStatusCodes.MovedPermanently, HttpStatusCodes.Redirection);
        Assert.Contains(HttpStatusCodes.Found, HttpStatusCodes.Redirection);
        Assert.Contains(HttpStatusCodes.NotModified, HttpStatusCodes.Redirection);
        Assert.Contains(HttpStatusCodes.TemporaryRedirect, HttpStatusCodes.Redirection);
        Assert.Contains(HttpStatusCodes.PermanentRedirect, HttpStatusCodes.Redirection);
    }

    [Fact]
    public void GivenStatusCodes_WhenAccessingClientErrorGroup_ThenContains4xxCodes()
    {
        Assert.Contains(HttpStatusCodes.BadRequest, HttpStatusCodes.ClientError);
        Assert.Contains(HttpStatusCodes.Unauthorized, HttpStatusCodes.ClientError);
        Assert.Contains(HttpStatusCodes.Forbidden, HttpStatusCodes.ClientError);
        Assert.Contains(HttpStatusCodes.NotFound, HttpStatusCodes.ClientError);
        Assert.Contains(HttpStatusCodes.MethodNotAllowed, HttpStatusCodes.ClientError);
        Assert.Contains(HttpStatusCodes.Conflict, HttpStatusCodes.ClientError);
        Assert.Contains(HttpStatusCodes.UnprocessableEntity, HttpStatusCodes.ClientError);
        Assert.Contains(HttpStatusCodes.TooManyRequests, HttpStatusCodes.ClientError);
    }

    [Fact]
    public void GivenStatusCodes_WhenAccessingServerErrorGroup_ThenContains5xxCodes()
    {
        Assert.Contains(HttpStatusCodes.InternalServerError, HttpStatusCodes.ServerError);
        Assert.Contains(HttpStatusCodes.NotImplemented, HttpStatusCodes.ServerError);
        Assert.Contains(HttpStatusCodes.BadGateway, HttpStatusCodes.ServerError);
        Assert.Contains(HttpStatusCodes.ServiceUnavailable, HttpStatusCodes.ServerError);
        Assert.Contains(HttpStatusCodes.GatewayTimeout, HttpStatusCodes.ServerError);
    }

    [Fact]
    public void GivenStatusCodes_WhenAccessingAllGroup_ThenContainsAllCodes()
    {
        var expectedCount = HttpStatusCodes.Success.Length + HttpStatusCodes.Redirection.Length +
                            HttpStatusCodes.ClientError.Length + HttpStatusCodes.ServerError.Length;

        Assert.Equal(expectedCount, HttpStatusCodes.All.Length);

        foreach (var code in HttpStatusCodes.Success)
        {
            Assert.Contains(code, HttpStatusCodes.All);
        }

        foreach (var code in HttpStatusCodes.Redirection)
        {
            Assert.Contains(code, HttpStatusCodes.All);
        }

        foreach (var code in HttpStatusCodes.ClientError)
        {
            Assert.Contains(code, HttpStatusCodes.All);
        }

        foreach (var code in HttpStatusCodes.ServerError)
        {
            Assert.Contains(code, HttpStatusCodes.All);
        }
    }
}

