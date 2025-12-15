namespace ArturRios.Util.Http;

/// <summary>
/// A lightweight HTTP gateway that wraps <see cref="HttpClient"/> and provides typed helpers
/// for common HTTP verbs, returning <see cref="HttpOutput{TBody}"/> with deserialized content.
/// </summary>
public class HttpGateway(HttpClient client)
{
    /// <summary>
    /// Gets the underlying <see cref="HttpClient"/> used to perform HTTP requests.
    /// </summary>
    public HttpClient Client { get; } = client;

    /// <summary>
    /// Sends a GET request to the specified route and deserializes the response body.
    /// </summary>
    /// <typeparam name="TBody">The expected response body type.</typeparam>
    /// <param name="route">The relative or absolute route for the request.</param>
    /// <returns>An <see cref="HttpOutput{TBody}"/> containing status, headers, and the deserialized body.</returns>
    public async Task<HttpOutput<TBody?>> GetAsync<TBody>(string route)
    {
        var response = await Client.GetAsync(route);

        return await ResolveResponseAsync<TBody?>(response);
    }

    /// <summary>
    /// Sends a PATCH request with an optional payload, serialized as JSON.
    /// </summary>
    /// <typeparam name="TBody">The expected response body type.</typeparam>
    /// <param name="route">The route to send the request to.</param>
    /// <param name="payloadObject">An optional object to be serialized as JSON for the request body.</param>
    /// <returns>An <see cref="HttpOutput{TBody}"/> with the response information and body.</returns>
    public async Task<HttpOutput<TBody?>> PatchAsync<TBody>(string route, object? payloadObject = null)
    {
        var payload = payloadObject?.ToJsonStringContent();

        var response = await Client.PatchAsync(route, payload);

        return await ResolveResponseAsync<TBody?>(response);
    }

    /// <summary>
    /// Sends a POST request with an optional payload, serialized as JSON.
    /// </summary>
    /// <typeparam name="TBody">The expected response body type.</typeparam>
    /// <param name="route">The route to send the request to.</param>
    /// <param name="payloadObject">An optional object to be serialized as JSON for the request body.</param>
    /// <returns>An <see cref="HttpOutput{TBody}"/> with the response information and body.</returns>
    public async Task<HttpOutput<TBody?>> PostAsync<TBody>(string route, object? payloadObject = null)
    {
        var payload = payloadObject?.ToJsonStringContent();

        var response = await Client.PostAsync(route, payload);

        return await ResolveResponseAsync<TBody?>(response);
    }

    /// <summary>
    /// Sends a PUT request with an optional payload, serialized as JSON.
    /// </summary>
    /// <typeparam name="TBody">The expected response body type.</typeparam>
    /// <param name="route">The route to send the request to.</param>
    /// <param name="payloadObject">An optional object to be serialized as JSON for the request body.</param>
    /// <returns>An <see cref="HttpOutput{TBody}"/> with the response information and body.</returns>
    public async Task<HttpOutput<TBody?>> PutAsync<TBody>(string route, object? payloadObject = null)
    {
        var payload = payloadObject?.ToJsonStringContent();

        var response = await Client.PutAsync(route, payload);

        return await ResolveResponseAsync<TBody?>(response);
    }

    /// <summary>
    /// Sends a DELETE request to the specified route and deserializes the response body.
    /// </summary>
    /// <typeparam name="TBody">The expected response body type.</typeparam>
    /// <param name="route">The route to send the request to.</param>
    /// <returns>An <see cref="HttpOutput{TBody}"/> with the response information and body.</returns>
    public async Task<HttpOutput<TBody?>> DeleteAsync<TBody>(string route)
    {
        var response = await Client.DeleteAsync(route);

        return await ResolveResponseAsync<TBody?>(response);
    }

    /// <summary>
    /// Resolves the HTTP response into an <see cref="HttpOutput{TBody}"/> and reads the content.
    /// </summary>
    /// <typeparam name="TBody">The target type to deserialize the response content to.</typeparam>
    /// <param name="response">The <see cref="HttpResponseMessage"/> to process.</param>
    /// <returns>An <see cref="HttpOutput{TBody}"/> with populated status, headers, and body.</returns>
    private static async Task<HttpOutput<TBody?>> ResolveResponseAsync<TBody>(HttpResponseMessage response)
    {
        var output = new HttpOutput<TBody?>(response);

        await output.ReadContent();

        return output;
    }
}
