namespace ArturRios.Util.Http;

/// <summary>
/// A lightweight HTTP gateway that wraps <see cref="HttpClient"/> and provides typed helpers
/// for common HTTP verbs, returning <see cref="HttpOutput{TBody}"/> with deserialized content.
/// </summary>
/// <remarks>
/// The response body is fully buffered into the returned <see cref="HttpOutput{TBody}"/> and the
/// underlying <see cref="HttpResponseMessage"/> is disposed before the call returns, so no connection is
/// left pinned by a caller that ignores the result.
/// </remarks>
/// <param name="client">The client used to perform HTTP requests.</param>
/// <exception cref="ArgumentNullException"><paramref name="client"/> is <c>null</c>.</exception>
public class HttpGateway(HttpClient client)
{
    /// <summary>
    /// Gets the underlying <see cref="HttpClient"/> used to perform HTTP requests.
    /// </summary>
    public HttpClient Client { get; } = client ?? throw new ArgumentNullException(nameof(client));

    /// <summary>
    /// Sends a GET request to the specified route and deserializes the response body.
    /// </summary>
    /// <typeparam name="TBody">The expected response body type.</typeparam>
    /// <param name="route">The relative or absolute route for the request.</param>
    /// <param name="cancellationToken">Cancels the request and the body read.</param>
    /// <returns>An <see cref="HttpOutput{TBody}"/> containing status, headers, and the deserialized body.</returns>
    public Task<HttpOutput<TBody?>> GetAsync<TBody>(string route, CancellationToken cancellationToken = default) =>
        SendAsync<TBody>(() => Client.GetAsync(route, cancellationToken), cancellationToken);

    /// <summary>
    /// Sends a PATCH request with an optional payload, serialized as JSON.
    /// </summary>
    /// <typeparam name="TBody">The expected response body type.</typeparam>
    /// <param name="route">The route to send the request to.</param>
    /// <param name="payloadObject">An optional object to be serialized as JSON for the request body.</param>
    /// <param name="cancellationToken">Cancels the request and the body read.</param>
    /// <returns>An <see cref="HttpOutput{TBody}"/> with the response information and body.</returns>
    public Task<HttpOutput<TBody?>> PatchAsync<TBody>(
        string route,
        object? payloadObject = null,
        CancellationToken cancellationToken = default) =>
        SendAsync<TBody>(() => Client.PatchAsync(route, Serialize(payloadObject), cancellationToken), cancellationToken);

    /// <summary>
    /// Sends a POST request with an optional payload, serialized as JSON.
    /// </summary>
    /// <typeparam name="TBody">The expected response body type.</typeparam>
    /// <param name="route">The route to send the request to.</param>
    /// <param name="payloadObject">An optional object to be serialized as JSON for the request body.</param>
    /// <param name="cancellationToken">Cancels the request and the body read.</param>
    /// <returns>An <see cref="HttpOutput{TBody}"/> with the response information and body.</returns>
    public Task<HttpOutput<TBody?>> PostAsync<TBody>(
        string route,
        object? payloadObject = null,
        CancellationToken cancellationToken = default) =>
        SendAsync<TBody>(() => Client.PostAsync(route, Serialize(payloadObject), cancellationToken), cancellationToken);

    /// <summary>
    /// Sends a PUT request with an optional payload, serialized as JSON.
    /// </summary>
    /// <typeparam name="TBody">The expected response body type.</typeparam>
    /// <param name="route">The route to send the request to.</param>
    /// <param name="payloadObject">An optional object to be serialized as JSON for the request body.</param>
    /// <param name="cancellationToken">Cancels the request and the body read.</param>
    /// <returns>An <see cref="HttpOutput{TBody}"/> with the response information and body.</returns>
    public Task<HttpOutput<TBody?>> PutAsync<TBody>(
        string route,
        object? payloadObject = null,
        CancellationToken cancellationToken = default) =>
        SendAsync<TBody>(() => Client.PutAsync(route, Serialize(payloadObject), cancellationToken), cancellationToken);

    /// <summary>
    /// Sends a DELETE request to the specified route and deserializes the response body.
    /// </summary>
    /// <typeparam name="TBody">The expected response body type.</typeparam>
    /// <param name="route">The route to send the request to.</param>
    /// <param name="cancellationToken">Cancels the request and the body read.</param>
    /// <returns>An <see cref="HttpOutput{TBody}"/> with the response information and body.</returns>
    public Task<HttpOutput<TBody?>> DeleteAsync<TBody>(string route, CancellationToken cancellationToken = default) =>
        SendAsync<TBody>(() => Client.DeleteAsync(route, cancellationToken), cancellationToken);

    /// <summary>
    /// Serializes an optional payload, leaving a null payload as no request body at all rather than the
    /// JSON literal <c>null</c>.
    /// </summary>
    private static HttpContent? Serialize(object? payloadObject) => payloadObject?.ToJsonStringContent();

    /// <summary>
    /// Runs a request, buffers the response into an <see cref="HttpOutput{TBody}"/> and disposes the
    /// response message.
    /// </summary>
    private static async Task<HttpOutput<TBody?>> SendAsync<TBody>(
        Func<Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        using var response = await send().ConfigureAwait(false);

        var output = new HttpOutput<TBody?>(response);

        await output.ReadContentAsync(cancellationToken).ConfigureAwait(false);

        return output;
    }
}
