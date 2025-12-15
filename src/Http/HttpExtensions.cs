using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;

namespace ArturRios.Util.Http;

/// <summary>
/// Provides HTTP-related extension helpers for serializing objects to HTTP content.
/// </summary>
public static class HttpExtensions
{
    /// <summary>
    /// Adds JSON serialization helpers to <see cref="object"/> instances for HTTP requests.
    /// </summary>
    /// <param name="object">The target object to extend.</param>
    extension(object @object)
    {
        /// <summary>
        /// Serializes the current object instance to JSON and wraps it in a <see cref="StringContent"/>
        /// with UTF-8 encoding and <see cref="MediaTypeNames.Application.Json"/> media type.
        /// </summary>
        /// <returns>
        /// A <see cref="StringContent"/> instance containing the JSON representation of the object.
        /// </returns>
        public StringContent ToJsonStringContent()
        {
            var json = JsonConvert.SerializeObject(@object);

            return new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        }
    }
}
