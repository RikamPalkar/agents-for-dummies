# HttpClientExt.cs Walkthrough

## Purpose

`HttpClientExt.cs` adds a helper method to `HttpClient` so your code can:

1. Send an HTTP GET request
2. Validate that the response was successful
3. Parse the response body as JSON
4. Return the JSON as a `JsonDocument`

This keeps tool code clean because instead of repeating these steps everywhere, you call one method.

---

## Source Code

```csharp
using System.Text.Json;

namespace mcp_server_dotnet;

internal static class HttpClǐentExt
{
    public static async Task<JsonDocument> ReadJsonDocumentAsync(this HttpClient client, string requestUri)
    {̌
        using var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    }
}
```

---

## Line-by-Line Explanation

```csharp
using System.Text.Json;
```
Imports .NET JSON APIs. `JsonDocument` lives in this namespace.

> **.NET concept**
> `System.Text.Json` is the built-in JSON library in modern .NET. It is fast, memory-efficient, and does not require an external package for common scenarios.

---

```csharp
namespace mcp_server_dotnet;
```
Defines the namespace for this file.

> **C# concept - file-scoped namespace**
> This syntax (ending with `;`) is a file-scoped namespace. It avoids an extra outer brace block and keeps files flatter and cleaner.

---

```csharp
internal static class HttpClientExt
```
Declares a class that contains extension methods for `HttpClient`.

> **C# concept - `internal`**
> `internal` means this type is visible only inside this project/assembly. Other projects cannot access it directly.

> **C# concept - `static class`**
> Extension methods must be inside a `static` class. You do not create instances of this class.

> **C# concept - extension class naming**
> `HttpClientExt` is a common naming style, but many teams also use names like `HttpClientExtensions` for clarity.

---

```csharp
public static async Task<JsonDocument> ReadJsonDocumentAsync(this HttpClient client, string requestUri)
```
Defines the extension method.

Breakdown:

- `public`: callable from anywhere in this assembly where namespace is in scope
- `static`: required for extension methods
- `async`: method uses `await`
- `Task<JsonDocument>`: asynchronous return type, eventually gives a `JsonDocument`
- `this HttpClient client`: turns this into an extension method on `HttpClient`
- `string requestUri`: endpoint path or URL to request

> **C# concept - extension method parameter**
> The first parameter marked with `this` is the type being extended. Because of this, you can call:
>
> `await client.ReadJsonDocumentAsync("/points/39.7456,-97.0892");`
>
> instead of a static-style call.

> **C# concept - async return types**
> Async methods commonly return `Task` or `Task<T>`. `Task<T>` means "work in progress that eventually produces a T".

---

```csharp
using var response = await client.GetAsync(requestUri);
```
Sends an HTTP GET request and stores the response.

> **C# concept - `using var`**
> `HttpResponseMessage` holds unmanaged resources. `using var` ensures it is disposed automatically when the method exits, even if an exception occurs.

> **.NET concept - `HttpClient.GetAsync`**
> This performs network I/O asynchronously. `await` does not block the thread while waiting for the server.

---

```csharp
response.EnsureSuccessStatusCode();
```
Throws an exception if the status code is not success (outside 200-299 range).

> **.NET concept - fail fast on HTTP errors**
> This line prevents parsing error pages as JSON accidentally. If the API returns 404/500, the method fails immediately with an `HttpRequestException`.

---

```csharp
return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
```
Reads the response body stream and parses it into a `JsonDocument`.

> **C# concept - nested `await`**
> First `await response.Content.ReadAsStreamAsync()` gets the response stream.
> Then `await JsonDocument.ParseAsync(...)` parses that stream.
>
> The line is compact but does two async operations in sequence.

> **.NET concept - streaming vs string loading**
> Parsing from stream avoids creating a large intermediate string in memory (`ReadAsStringAsync`), which is often better for performance and memory usage.

---

## How It Works at Runtime

1. Caller invokes `ReadJsonDocumentAsync` on an existing `HttpClient`.
2. The method sends an HTTP GET request to `requestUri`.
3. If response status is non-success, an exception is thrown.
4. Response body stream is parsed to JSON.
5. Parsed `JsonDocument` is returned to caller.
6. `response` is disposed automatically due to `using var`.

---

## Why This Helper Is Useful

- Removes repeated boilerplate (GET + status check + parse)
- Keeps tool methods shorter and easier to read
- Gives consistent error handling behavior
- Encourages async + stream-based JSON parsing

---

## Important C# and .NET Concepts in This File

- Namespace management with `using`
- File-scoped namespaces
- Access modifiers (`internal`, `public`)
- Static classes and extension methods
- Async/await with `Task<T>`
- Resource disposal with `using var`
- HTTP error handling via `EnsureSuccessStatusCode`
- Efficient JSON parsing from streams

---

## Example Usage

```csharp
var json = await httpClient.ReadJsonDocumentAsync("/points/39.7456,-97.0892");
var root = json.RootElement;
```

Here:

- `json` is a `JsonDocument`
- `RootElement` is the top JSON node
- You can read properties with `GetProperty(...)`

---

## Beginner Notes

- This method does not catch exceptions. That is usually fine for helper methods; callers can decide how to handle failures.
- `JsonDocument` should be disposed by the caller when done, because it also uses pooled resources.
- If you often map JSON to C# classes, you can also use `JsonSerializer.Deserialize<T>(...)` for strongly typed models.
