using System.Text.Json;

namespace ArturRios.Util.Http;

/// <summary>
/// The <see cref="JsonSerializerOptions"/> shared by everything in this namespace.
/// </summary>
/// <remarks>
/// Property matching is case insensitive so that a server answering in camelCase binds to PascalCase
/// members without per-call configuration, which is also how Newtonsoft.Json behaved before this library
/// moved to <see cref="System.Text.Json"/>. Serialization keeps member names exactly as declared.
/// </remarks>
internal static class JsonDefaults
{
    /// <summary>
    /// Options used by <see cref="HttpExtensions"/> and <see cref="HttpOutput{TBody}"/>.
    /// </summary>
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
