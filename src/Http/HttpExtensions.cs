using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace ArturRios.Util.Http;

/// <summary>
/// Provides HTTP-related extension helpers for serializing objects to HTTP content.
/// </summary>
/// <remarks>
/// The extension is declared on a generic value rather than on <see cref="object"/>. Extending
/// <see cref="object"/> would attach <c>ToJsonStringContent</c> to every type in every file that imports
/// this namespace, including <see cref="int"/> and <see cref="string"/>.
/// </remarks>
public static class HttpExtensions
{
    /// <summary>
    /// Adds JSON serialization helpers for HTTP requests.
    /// </summary>
    /// <param name="value">The payload to serialize.</param>
    /// <typeparam name="TPayload">The payload type, used to drive serialization.</typeparam>
    extension<TPayload>(TPayload value)
    {
        /// <summary>
        /// Serializes the value to JSON and wraps it in a <see cref="StringContent"/>
        /// with UTF-8 encoding and <see cref="MediaTypeNames.Application.Json"/> media type.
        /// </summary>
        /// <returns>
        /// A <see cref="StringContent"/> instance containing the JSON representation of the value.
        /// </returns>
        public StringContent ToJsonStringContent()
        {
            var json = JsonSerializer.Serialize(value, JsonDefaults.Options);

            return new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        }
    }
}
