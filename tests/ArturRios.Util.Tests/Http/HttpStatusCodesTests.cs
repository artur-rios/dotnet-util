using ArturRios.Util.Http;

namespace ArturRios.Util.Tests.Http;

public class HttpStatusCodesTests
{
    [Fact]
    public void Should_Contain2xxCodes()
    {
        Assert.Contains(HttpStatusCodes.Ok, HttpStatusCodes.Success);
        Assert.Contains(HttpStatusCodes.Created, HttpStatusCodes.Success);
        Assert.Contains(HttpStatusCodes.NoContent, HttpStatusCodes.Success);
    }

    [Fact]
    public void Should_Contain4xxCodes()
    {
        Assert.Contains(HttpStatusCodes.BadRequest, HttpStatusCodes.ClientError);
        Assert.Contains(HttpStatusCodes.Unauthorized, HttpStatusCodes.ClientError);
        Assert.Contains(HttpStatusCodes.Forbidden, HttpStatusCodes.ClientError);
        Assert.Contains(HttpStatusCodes.NotFound, HttpStatusCodes.ClientError);
    }

    [Fact]
    public void Should_Contain5xxCodes()
    {
        Assert.Contains(HttpStatusCodes.InternalServerError, HttpStatusCodes.ServerError);
        Assert.Contains(HttpStatusCodes.NotImplemented, HttpStatusCodes.ServerError);
        Assert.Contains(HttpStatusCodes.BadGateway, HttpStatusCodes.ServerError);
    }

    [Fact]
    public void Should_AggregateAllGroups()
    {
        var expectedCount = HttpStatusCodes.Success.Length + HttpStatusCodes.ClientError.Length + HttpStatusCodes.ServerError.Length;

        Assert.Equal(expectedCount, HttpStatusCodes.All.Length);

        foreach (var code in HttpStatusCodes.Success)
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

