using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ArturRios.Util.Http;

/// <summary>
/// Represents a typed HTTP response output, including status, headers, and a deserialized body.
/// </summary>
/// <typeparam name="TBody">The type of the deserialized response body.</typeparam>
public class HttpOutput<TBody>(HttpResponseMessage responseMessage)
{
    /// <summary>
    /// Gets the HTTP status code returned by the server.
    /// </summary>
    public HttpStatusCode StatusCode { get; } = responseMessage.StatusCode;

    /// <summary>
    /// Gets the HTTP response headers. Content headers such as Content-Type are exposed separately by
    /// <see cref="ContentHeaders"/>.
    /// </summary>
    public HttpResponseHeaders Headers { get; } = responseMessage.Headers;

    /// <summary>
    /// Gets the HTTP content headers, such as Content-Type and Content-Length.
    /// </summary>
    public HttpContentHeaders ContentHeaders { get; } = responseMessage.Content.Headers;

    /// <summary>
    /// Gets the raw response body, exactly as the server sent it.
    /// </summary>
    /// <remarks>
    /// Populated by <see cref="ReadContentAsync"/>. Useful when <see cref="Body"/> is <c>null</c> because
    /// the payload could not be bound to <typeparamref name="TBody"/> — an HTML error page, for example.
    /// </remarks>
    public string? RawBody { get; private set; }

    /// <summary>
    /// Gets the deserialized response body, or <c>null</c> when the payload could not be bound to
    /// <typeparamref name="TBody"/>.
    /// </summary>
    public TBody? Body { get; private set; }

    /// <summary>
    /// Indicates whether the response carries a 2xx status code.
    /// </summary>
    public bool IsSuccess => (int)StatusCode is >= 200 and <= 299;

    /// <summary>
    /// Reads the response content as a string and deserializes it into <typeparamref name="TBody"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancels reading the response body.</param>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
    /// <remarks>
    /// Any malformed or mismatched payload leaves <see cref="Body"/> at its default and the response text
    /// in <see cref="RawBody"/>. Both a syntax error and a shape mismatch — an array where an object was
    /// expected, say — are handled: they are distinct exception types, and catching only the first used to
    /// let the second escape to the caller.
    /// </remarks>
    public async Task ReadContentAsync(CancellationToken cancellationToken = default)
    {
        RawBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            Body = JsonSerializer.Deserialize<TBody>(RawBody, JsonDefaults.Options);
        }
        catch (JsonException)
        {
            Body = default;
        }
    }
}
