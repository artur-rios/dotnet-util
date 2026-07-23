namespace ArturRios.Util.Http;

/// <summary>
/// Provides common HTTP status code constants and grouped collections for convenience.
/// </summary>
public static class HttpStatusCodes
{
    /// <summary>HTTP 200 - OK.</summary>
    public const int Ok = 200;
    /// <summary>HTTP 201 - Created.</summary>
    public const int Created = 201;
    /// <summary>HTTP 202 - Accepted.</summary>
    public const int Accepted = 202;
    /// <summary>HTTP 204 - No Content.</summary>
    public const int NoContent = 204;

    /// <summary>HTTP 301 - Moved Permanently.</summary>
    public const int MovedPermanently = 301;
    /// <summary>HTTP 302 - Found.</summary>
    public const int Found = 302;
    /// <summary>HTTP 304 - Not Modified.</summary>
    public const int NotModified = 304;
    /// <summary>HTTP 307 - Temporary Redirect.</summary>
    public const int TemporaryRedirect = 307;
    /// <summary>HTTP 308 - Permanent Redirect.</summary>
    public const int PermanentRedirect = 308;

    /// <summary>HTTP 400 - Bad Request.</summary>
    public const int BadRequest = 400;
    /// <summary>HTTP 401 - Unauthorized.</summary>
    public const int Unauthorized = 401;
    /// <summary>HTTP 403 - Forbidden.</summary>
    public const int Forbidden = 403;
    /// <summary>HTTP 404 - Not Found.</summary>
    public const int NotFound = 404;
    /// <summary>HTTP 405 - Method Not Allowed.</summary>
    public const int MethodNotAllowed = 405;
    /// <summary>HTTP 409 - Conflict.</summary>
    public const int Conflict = 409;
    /// <summary>HTTP 422 - Unprocessable Entity.</summary>
    public const int UnprocessableEntity = 422;
    /// <summary>HTTP 429 - Too Many Requests.</summary>
    public const int TooManyRequests = 429;

    /// <summary>HTTP 500 - Internal Server Error.</summary>
    public const int InternalServerError = 500;
    /// <summary>HTTP 501 - Not Implemented.</summary>
    public const int NotImplemented = 501;
    /// <summary>HTTP 502 - Bad Gateway.</summary>
    public const int BadGateway = 502;
    /// <summary>HTTP 503 - Service Unavailable.</summary>
    public const int ServiceUnavailable = 503;
    /// <summary>HTTP 504 - Gateway Timeout.</summary>
    public const int GatewayTimeout = 504;

    /// <summary>
    /// Group containing success status codes (2xx).
    /// </summary>
    public static int[] Success => [Ok, Created, Accepted, NoContent];

    /// <summary>
    /// Group containing redirection status codes (3xx).
    /// </summary>
    public static int[] Redirection => [MovedPermanently, Found, NotModified, TemporaryRedirect, PermanentRedirect];

    /// <summary>
    /// Group containing client error status codes (4xx).
    /// </summary>
    public static int[] ClientError =>
        [BadRequest, Unauthorized, Forbidden, NotFound, MethodNotAllowed, Conflict, UnprocessableEntity, TooManyRequests];

    /// <summary>
    /// Group containing server error status codes (5xx).
    /// </summary>
    public static int[] ServerError => [InternalServerError, NotImplemented, BadGateway, ServiceUnavailable, GatewayTimeout];

    /// <summary>
    /// Convenience property containing all supported status codes.
    /// </summary>
    public static int[] All => [..Success, ..Redirection, ..ClientError, ..ServerError];
}
