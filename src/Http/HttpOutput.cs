using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace ArturRios.Util.Http;

/// <summary>
/// Represents a typed HTTP response output, including status, headers, and a deserialized body.
/// </summary>
/// <typeparam name="TBody">The type of the deserialized response body.</typeparam>
public class HttpOutput<TBody>(HttpResponseMessage responseMessage)
{
    /// <summary>
    /// Gets or sets the HTTP status code returned by the server.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; } = responseMessage.StatusCode;

    /// <summary>
    /// Gets or sets the HTTP response headers.
    /// </summary>
    public HttpResponseHeaders Headers { get; set; } = responseMessage.Headers;

    /// <summary>
    /// Gets or sets the deserialized response body.
    /// </summary>
    public TBody? Body { get; set; }

    /// <summary>
    /// Reads the response content as a string and deserializes it into <typeparamref name="TBody"/>.
    /// </summary>
    public async Task ReadContent()
    {
        var body = await responseMessage.Content.ReadAsStringAsync();

        try
        {
            Body = JsonConvert.DeserializeObject<TBody>(body);
        }
        catch (JsonReaderException)
        {
            Body = default;
        }
    }
}
