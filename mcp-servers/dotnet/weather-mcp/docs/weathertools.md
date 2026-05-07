# WeatherTools.cs Walkthrough

## Purpose

`WeatherTools.cs` defines the MCP tools that an AI client can call. In this file, there are two tools:

1. `GetAlerts` - returns active weather alerts for a US state
2. `GetForecast` - returns the forecast for a latitude/longitude

So this file is your "business logic" layer: it calls the weather API, reads JSON, and formats human-readable output.

---

## Source Code

```csharp
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using ModelContextProtocol.Server;
namespace mcp_server_dotnet;

[McpServerToolType]

public static class WeatherTools
{
    [McpServerTool, Description("Get weather alerts for a US state code.")]
    public static async Task<string> GetAlerts(
        HttpClient client,
        [Description("The US state code to get alerts for.")] string state)
    {
        using var jsonDocument = await client.ReadJsonDocumentAsync($"/alerts/active/area/{state}");
        var jsonElement = jsonDocument.RootElement;
        var alerts = jsonElement.GetProperty("features").EnumerateArray();

        if (!alerts.Any())
        {
            return "No active alerts for this state.";
        }

        return string.Join("\n--\n", alerts.Select(alert =>
        {
            JsonElement properties = alert.GetProperty("properties");
            return $"""
                    Event: {properties.GetProperty("event").GetString()}
                    Area: {properties.GetProperty("areaDesc").GetString()}
                    Severity: {properties.GetProperty("severity").GetString()}
                    Description: {properties.GetProperty("description").GetString()}
                    Instruction: {properties.GetProperty("instruction").GetString()}
                    """;
        }));
    }

    [McpServerTool, Description("Get weather forecast for a location.")]
    public static async Task<string> GetForecast(
        HttpClient client,
        [Description("Latitude of the location.")] double latitude,
        [Description("Longitude of the location.")] double longitude)
    {
        var pointUrl = string.Create(CultureInfo.InvariantCulture, $"/points/{latitude},{longitude}");
        using var jsonDocument = await client.ReadJsonDocumentAsync(pointUrl);
        var forecastUrl = jsonDocument.RootElement.GetProperty("properties").GetProperty("forecast").GetString()
            ?? throw new Exception($"No forecast URL provided by {client.BaseAddress}points/{latitude},{longitude}");

        using var forecastDocument = await client.ReadJsonDocumentAsync(forecastUrl);
        var periods = forecastDocument.RootElement.GetProperty("properties").GetProperty("periods").EnumerateArray();

        return string.Join("\n---\n", periods.Select(period => $"""
                {period.GetProperty("name").GetString()}
                Temperature: {period.GetProperty("temperature").GetInt32()}°F
                Wind: {period.GetProperty("windSpeed").GetString()} {period.GetProperty("windDirection").GetString()}
                Forecast: {period.GetProperty("detailedForecast").GetString()}
                """));
    }
}
```

---

## Line-by-Line Explanation

```csharp
using System.ComponentModel;
```
Imports `DescriptionAttribute`, used to describe tools and parameters.

> **C# concept - attributes**
> Attributes are metadata attached with square brackets (`[Something]`). Frameworks can read these at runtime via reflection.

---

```csharp
using System.Globalization;
```
Imports culture-related APIs (`CultureInfo`) used for safe numeric formatting.

> **.NET concept - globalization/culture**
> Different locales format decimals differently (`12.34` vs `12,34`). Weather API URLs must always use `.` as decimal separator, so culture-invariant formatting is important.

---

```csharp
using System.Text.Json;
```
Imports JSON APIs like `JsonDocument` and `JsonElement`.

---

```csharp
using ModelContextProtocol.Server;
```
Imports MCP server attributes (`McpServerToolType`, `McpServerTool`).

---

```csharp
namespace mcp_server_dotnet;
```
File-scoped namespace for this project.

---

```csharp
[McpServerToolType]
public static class WeatherTools
```
Marks this class as containing MCP tool methods.

> **C# concept - static class**
> A `static class` cannot be instantiated. It only contains static members.

> **MCP concept**
> The MCP framework scans the assembly for this marker and then inspects methods with `[McpServerTool]`.

---

```csharp
[McpServerTool, Description("Get weather alerts for a US state code.")]
public static async Task<string> GetAlerts(...)
```
Defines a callable MCP tool named `GetAlerts`.

> **C# concept - async `Task<string>`**
> This method runs asynchronously and eventually returns a `string`.

> **MCP concept - tool metadata**
> `Description(...)` helps the AI understand when and how to use this tool.

---

```csharp
HttpClient client,
[Description("The US state code to get alerts for.")] string state
```
Method parameters:

- `client`: injected by DI
- `state`: user-provided argument (example: `CA`, `TX`, `NY`)

> **.NET concept - Dependency Injection in parameters**
> MCP can resolve framework/service parameters like `HttpClient`, while model/user arguments are provided from tool call input.

---

```csharp
using var jsonDocument = await client.ReadJsonDocumentAsync($"/alerts/active/area/{state}");
```
Calls your extension method to GET and parse JSON for the state alerts endpoint.

> **C# concept - string interpolation**
> `$"...{state}..."` inserts variable values directly into strings.

> **C# concept - disposal**
> `using var` ensures `JsonDocument` is disposed at end of scope.

---

```csharp
var jsonElement = jsonDocument.RootElement;
var alerts = jsonElement.GetProperty("features").EnumerateArray();
```
Navigates JSON: root -> `features` array -> iterable sequence.

> **.NET concept - DOM-style JSON parsing**
> `JsonDocument`/`JsonElement` let you traverse JSON without mapping to C# classes.

---

```csharp
if (!alerts.Any())
{
    return "No active alerts for this state.";
}
```
If array has no items, return a friendly message.

> **C# concept - LINQ**
> `Any()` is a LINQ method that checks whether sequence has at least one item.

---

```csharp
return string.Join("\n--\n", alerts.Select(alert =>
{
    JsonElement properties = alert.GetProperty("properties");
    return $"""
            Event: {properties.GetProperty("event").GetString()}
            Area: {properties.GetProperty("areaDesc").GetString()}
            Severity: {properties.GetProperty("severity").GetString()}
            Description: {properties.GetProperty("description").GetString()}
            Instruction: {properties.GetProperty("instruction").GetString()}
            """;
}));
```
Transforms each alert item into a formatted block of text, then joins blocks with separators.

> **C# concept - `Select` projection**
> `Select` maps each input item to a new output value.

> **C# concept - raw interpolated string literal**
> `$""" ... """` allows multiline strings with interpolation and less escaping noise.

> **C# concept - collection-to-string formatting**
> `string.Join` combines many strings into one response.

---

```csharp
[McpServerTool, Description("Get weather forecast for a location.")]
public static async Task<string> GetForecast(...)
```
Defines second MCP tool for forecast lookups.

---

```csharp
[Description("Latitude of the location.")] double latitude,
[Description("Longitude of the location.")] double longitude
```
Accepts coordinates as `double` values.

> **C# concept - `double`**
> `double` is a floating-point numeric type used for decimals (like coordinates).

---

```csharp
var pointUrl = string.Create(CultureInfo.InvariantCulture, $"/points/{latitude},{longitude}");
```
Builds URL path using invariant culture, ensuring decimal format is API-safe.

> **Why this matters**
> If system culture uses comma decimals, plain interpolation might generate invalid paths. Invariant culture prevents that.

---

```csharp
using var jsonDocument = await client.ReadJsonDocumentAsync(pointUrl);
var forecastUrl = jsonDocument.RootElement.GetProperty("properties").GetProperty("forecast").GetString()
    ?? throw new Exception($"No forecast URL provided by {client.BaseAddress}points/{latitude},{longitude}");
```
Calls `/points/{lat},{lon}` endpoint, then reads the `properties.forecast` URL from JSON.

If `forecast` is null, throws an exception immediately.

> **C# concept - null-coalescing throw**
> `value ?? throw ...` means: use `value` if non-null, otherwise throw.

---

```csharp
using var forecastDocument = await client.ReadJsonDocumentAsync(forecastUrl);
var periods = forecastDocument.RootElement.GetProperty("properties").GetProperty("periods").EnumerateArray();
```
Fetches forecast JSON using `forecastUrl`, then gets `periods` array (time blocks like Tonight, Monday, Monday Night).

---

```csharp
return string.Join("\n---\n", periods.Select(period => $"""
        {period.GetProperty("name").GetString()}
        Temperature: {period.GetProperty("temperature").GetInt32()}°F
        Wind: {period.GetProperty("windSpeed").GetString()} {period.GetProperty("windDirection").GetString()}
        Forecast: {period.GetProperty("detailedForecast").GetString()}
        """));
```
Formats each forecast period into text and joins all periods into one response string.

---

## Runtime Flow Summary

### GetAlerts flow

1. Receive `state` input from tool call.
2. Call weather API alerts endpoint.
3. Parse JSON `features` array.
4. If empty: return "No active alerts".
5. Else: format each alert and return combined text.

### GetForecast flow

1. Receive `latitude` and `longitude`.
2. Call `/points/{lat},{lon}` to discover forecast URL.
3. Validate forecast URL exists.
4. Call forecast URL.
5. Parse forecast periods and format text output.

---

## C# and .NET Concepts Used in This File

- Attributes and reflection-driven frameworks
- Static classes and static methods
- Async/await and `Task<T>`
- Dependency Injection (`HttpClient` parameter)
- String interpolation and raw string literals
- LINQ (`Any`, `Select`)
- JSON DOM parsing (`JsonDocument`, `JsonElement`)
- Culture-invariant formatting for APIs
- Null handling with `?? throw`
- Resource cleanup with `using var`

---

## Beginner Notes and Practical Tips

- API shape can change. `GetProperty(...)` throws if property is missing, so production code often uses safer checks (`TryGetProperty`).
- This code builds human-readable text. For richer clients, returning structured objects can be better.
- Exceptions are currently allowed to bubble up, which is okay for a simple server. Later, you can add tool-level error handling and clearer messages.
- Your helper `ReadJsonDocumentAsync` already centralizes request + parse behavior, which is a good design choice.
