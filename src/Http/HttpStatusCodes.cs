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
    /// <summary>HTTP 204 - No Content.</summary>
    public const int NoContent = 204;

    /// <summary>HTTP 400 - Bad Request.</summary>
    public const int BadRequest = 400;
    /// <summary>HTTP 401 - Unauthorized.</summary>
    public const int Unauthorized = 401;
    /// <summary>HTTP 403 - Forbidden.</summary>
    public const int Forbidden = 403;
    /// <summary>HTTP 404 - Not Found.</summary>
    public const int NotFound = 404;

    /// <summary>HTTP 500 - Internal Server Error.</summary>
    public const int InternalServerError = 500;
    /// <summary>HTTP 501 - Not Implemented.</summary>
    public const int NotImplemented = 501;
    /// <summary>HTTP 502 - Bad Gateway.</summary>
    public const int BadGateway = 502;

    /// <summary>
    /// Group containing success status codes (2xx).
    /// </summary>
    public static int[] Success => [Ok, Created, NoContent];

    /// <summary>
    /// Group containing client error status codes (4xx).
    /// </summary>
    public static int[] ClientError => [BadRequest, Unauthorized, Forbidden, NotFound];

    /// <summary>
    /// Group containing server error status codes (5xx).
    /// </summary>
    public static int[] ServerError => [InternalServerError, NotImplemented, BadGateway];

    /// <summary>
    /// Convenience property containing all supported status codes.
    /// </summary>
    public static int[] All => [..Success, ..ClientError, ..ServerError];
}
