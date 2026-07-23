+++
title          = "Http"
show_nav       = true
nav_back_label = "Hashing"
nav_back_url   = "/dotnet-util/hashing"
nav_next_label = "IO"
nav_next_url   = "/dotnet-util/io"
+++

## Features

- `HttpGateway`: wraps `HttpClient` with typed async methods for GET, POST, PUT, PATCH, and DELETE. Each method deserializes the response body into the specified type and returns an `HttpOutput<TBody>`.
- `HttpOutput<TBody>`: typed HTTP response container carrying the status code, response headers, and deserialized body.
- `HttpExtensions`: extension on `object` providing `ToJsonStringContent()` to serialize any object into a `StringContent` suitable for request payloads.
- `HttpStatusCodes`: static constants for common HTTP status codes (200, 201, 202, 204, 301, 302, 304, 307, 308, 400, 401, 403, 404, 405, 409, 422, 429, 500, 501, 502, 503, 504) plus grouped collections (`Success`, `Redirection`, `ClientError`, `ServerError`, `All`).

## Class Diagram

```mermaid
classDiagram
    namespace Http {
        class HttpGateway {
            +HttpClient Client
            +Task~HttpOutput~TBody~~ GetAsync~TBody~(string route)
            +Task~HttpOutput~TBody~~ PostAsync~TBody~(string route, object? payloadObject)
            +Task~HttpOutput~TBody~~ PutAsync~TBody~(string route, object? payloadObject)
            +Task~HttpOutput~TBody~~ PatchAsync~TBody~(string route, object? payloadObject)
            +Task~HttpOutput~TBody~~ DeleteAsync~TBody~(string route)
        }
        class HttpOutput~TBody~ {
            +HttpStatusCode StatusCode
            +HttpResponseHeaders Headers
            +TBody? Body
            +Task ReadContent()
        }
        class HttpExtensions {
            <<static>>
            +StringContent ToJsonStringContent(object @object)
        }
        class HttpStatusCodes {
            <<static>>
            +const int Ok
            +const int Created
            +const int Accepted
            +const int NoContent
            +const int MovedPermanently
            +const int Found
            +const int NotModified
            +const int TemporaryRedirect
            +const int PermanentRedirect
            +const int BadRequest
            +const int Unauthorized
            +const int Forbidden
            +const int NotFound
            +const int MethodNotAllowed
            +const int Conflict
            +const int UnprocessableEntity
            +const int TooManyRequests
            +const int InternalServerError
            +const int NotImplemented
            +const int BadGateway
            +const int ServiceUnavailable
            +const int GatewayTimeout
            +int[] Success
            +int[] Redirection
            +int[] ClientError
            +int[] ServerError
            +int[] All
        }
    }
    HttpGateway --> HttpOutput : returns
    HttpGateway ..> HttpExtensions : uses
```

## Usage

### Basic GET request

```csharp
using ArturRios.Util.Http;

var client  = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
var gateway = new HttpGateway(client);

var output = await gateway.GetAsync<List<Product>>("/products");

if ((int)output.StatusCode == HttpStatusCodes.Ok)
{
    foreach (var product in output.Body!)
        Console.WriteLine(product.Name);
}
```

### POST with a payload

```csharp
var newProduct = new { Name = "Widget", Price = 9.99 };

var output = await gateway.PostAsync<Product>("/products", newProduct);

if ((int)output.StatusCode == HttpStatusCodes.Created)
    Console.WriteLine($"Created: {output.Body!.Id}");
```

### PUT and PATCH

```csharp
var update = new { Name = "Updated Widget" };

await gateway.PutAsync<Product>("/products/42", update);
await gateway.PatchAsync<Product>("/products/42", update);
```

### DELETE

```csharp
var output = await gateway.DeleteAsync<object>("/products/42");

if ((int)output.StatusCode == HttpStatusCodes.NoContent)
    Console.WriteLine("Deleted.");
```

### Status code groups

```csharp
int code = (int)output.StatusCode;

if (HttpStatusCodes.Success.Contains(code))
    Console.WriteLine("Request succeeded.");
else if (HttpStatusCodes.ClientError.Contains(code))
    Console.WriteLine("Client error — check your request.");
else if (HttpStatusCodes.ServerError.Contains(code))
    Console.WriteLine("Server error — try again later.");
```

### Serialize a payload manually

```csharp
var payload = new UpdateRequest { Name = "New Name" };
StringContent content = payload.ToJsonStringContent();
// UTF-8, application/json
```
