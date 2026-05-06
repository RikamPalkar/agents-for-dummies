# Build Your Own MCP Server with .NET: A Practical, MCP-Focused Guide

## 1. Introduction

Model Context Protocol (MCP) is an open protocol that lets AI clients (IDEs, assistants, chat apps, agents) call external tools in a standardized way. Instead of building one-off integrations for each AI platform, you expose capabilities once as an MCP server, and any MCP-compatible client can use them.

At a high level:

- The MCP client discovers available tools from your server.
- The model chooses a tool based on user intent.
- The client sends typed arguments to your server.
- Your server executes tool logic and returns structured results.

Real-world use cases:

- Developer tooling: search internal APIs, generate release notes, query CI status.
- Operations: check deployment health, inspect logs, trigger safe runbooks.
- Business systems: query CRM/ticketing data through audited tool functions.
- Data workflows: run domain-specific transformations from natural language prompts.

Official references:

- MCP specification and docs: https://modelcontextprotocol.io/
- MCP GitHub org: https://github.com/modelcontextprotocol
- .NET Generic Host docs: https://learn.microsoft.com/dotnet/core/extensions/generic-host

## 2. Prerequisites

Keep this section short and practical.

Required tools:

- .NET SDK 8.0+: https://dotnet.microsoft.com/download
- A code editor (Visual Studio, VS Code, Rider)
- Terminal access
- Basic HTTP/JSON familiarity

Knowledge assumptions:

- Basic C# syntax (`async/await`, classes, methods)
- Basic NuGet usage
- Basic command-line usage (`dotnet` CLI)

## 3. Project Setup

Create a new console app (MCP servers commonly run as long-lived console processes via stdio).

```bash
dotnet new console -n weather-mcp
cd weather-mcp
dotnet new sln -n mcp-server-dotnet
dotnet sln add weather-mcp.csproj
```

A minimal structure you will end up with:

```text
weather-mcp/
  Program.cs
  WeatherTools.cs
  HttpClientExt.cs
  appsettings.json
  weather-mcp.csproj
```

What each file does:

- `Program.cs`: host setup + MCP server registration + DI wiring
- `WeatherTools.cs`: MCP tool methods (the core contract surface)
- `HttpClientExt.cs`: reusable HTTP/JSON helper
- `appsettings.json`: runtime settings like API base URL and user agent

## 4. Installing Dependencies

Install required packages:

```bash
dotnet add package ModelContextProtocol
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Options.ConfigurationExtensions
```

Why these packages matter:

- `ModelContextProtocol`: MCP server framework APIs and attributes.
- `Microsoft.Extensions.Hosting`: Generic Host lifecycle + dependency injection container.
- `Microsoft.Extensions.Configuration.Json`: load settings from `appsettings.json`.
- `Microsoft.Extensions.Options.ConfigurationExtensions`: strongly typed configuration binding.

Restore/build:

```bash
dotnet restore
dotnet build
```

## 5. MCP Server Implementation (Main Focus)

This is the important part. We will map MCP concepts directly to code.

### 5.1 Full `Program.cs`

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using System.Net.Http.Headers;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<WeatherApiOptions>(
    builder.Configuration.GetSection(WeatherApiOptions.SectionName));

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<WeatherApiOptions>>().Value;

    var client = new HttpClient
    {
        BaseAddress = new Uri(options.BaseUrl)
    };

    client.DefaultRequestHeaders.UserAgent.Add(
        new ProductInfoHeaderValue(options.UserAgentProduct, options.UserAgentVersion));

    return client;
});

var app = builder.Build();
await app.RunAsync();

public sealed class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    public string BaseUrl { get; set; } = "https://api.weather.gov";
    public string UserAgentProduct { get; set; } = "weather-mcp";
    public string UserAgentVersion { get; set; } = "1.0";
}
```

### 5.2 Full `WeatherTools.cs`

```csharp
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace weather_mcp;

[McpServerToolType]
public static class WeatherTools
{
    [McpServerTool, Description("Get weather alerts for a US state code.")]
    public static async Task<string> GetAlerts(
        HttpClient client,
        [Description("Two-letter US state code (for example: CA, NY, TX).")]
        string state)
    {
        using var jsonDocument = await client.ReadJsonDocumentAsync($"/alerts/active/area/{state}");
        var alerts = jsonDocument.RootElement
            .GetProperty("features")
            .EnumerateArray()
            .ToList();

        if (alerts.Count == 0)
        {
            return "No active alerts for this state.";
        }

        return string.Join("\n--\n", alerts.Select(alert =>
        {
            var properties = alert.GetProperty("properties");
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
        var pointUrl = string.Create(
            CultureInfo.InvariantCulture,
            $"/points/{latitude},{longitude}");

        using var pointDoc = await client.ReadJsonDocumentAsync(pointUrl);
        var forecastUrl = pointDoc.RootElement
                .GetProperty("properties")
                .GetProperty("forecast")
                .GetString()
            ?? throw new InvalidOperationException("Forecast URL missing from points response.");

        using var forecastDoc = await client.ReadJsonDocumentAsync(forecastUrl);
        var periods = forecastDoc.RootElement
            .GetProperty("properties")
            .GetProperty("periods")
            .EnumerateArray();

        return string.Join("\n---\n", periods.Select(period => $"""
            {period.GetProperty("name").GetString()}
            Temperature: {period.GetProperty("temperature").GetInt32()} deg F
            Wind: {period.GetProperty("windSpeed").GetString()} {period.GetProperty("windDirection").GetString()}
            Forecast: {period.GetProperty("detailedForecast").GetString()}
            """));
    }
}
```

### 5.3 Full `HttpClientExt.cs`

```csharp
using System.Text.Json;

namespace weather_mcp;

internal static class HttpClientExt
{
    public static async Task<JsonDocument> ReadJsonDocumentAsync(this HttpClient client, string requestUri)
    {
        using var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    }
}
```

---

### 5.4 MCP Concept-to-Code Mapping (Critical)

#### A. Server initialization

Code:

```csharp
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
```

What this means in MCP terms:

- `AddMcpServer()`: registers MCP server services and protocol handlers.
- `WithStdioServerTransport()`: uses stdin/stdout transport, which most local MCP clients use.
- `WithToolsFromAssembly()`: reflection-based tool discovery from your compiled assembly.

#### B. Tool registration

Code:

```csharp
[McpServerToolType]
public static class WeatherTools
{
    [McpServerTool]
    public static async Task<string> GetForecast(...)
```

MCP mapping:

- `[McpServerToolType]`: marks a class as containing MCP-exposed tools.
- `[McpServerTool]`: marks callable operations that the client can invoke.
- Method name becomes the tool identity unless overridden by the library.

#### C. Request handling and invocation pipeline

Flow:

1. Client sends `tools/call` for a specific tool.
2. MCP runtime resolves parameters.
3. DI services (`HttpClient`) are injected.
4. User/model arguments (`latitude`, `longitude`, `state`) are bound.
5. Method executes.
6. Return value becomes tool result payload.

#### D. Input schema handling

With this style, input schema is derived from method signatures + attributes:

- `string state` => string field in tool input schema
- `double latitude` => numeric field
- `[Description(...)]` => schema field descriptions shown to LLM/tooling

Example conceptual input schema for `GetForecast`:

```json
{
  "type": "object",
  "properties": {
    "latitude": { "type": "number", "description": "Latitude of the location." },
    "longitude": { "type": "number", "description": "Longitude of the location." }
  },
  "required": ["latitude", "longitude"]
}
```

You usually do not handwrite this schema in the attribute-driven approach; the MCP .NET library infers it.

#### E. Output schema handling

In this implementation, tools return `string`, so the result is text content. For richer outputs, return structured objects (DTOs) so clients/models can consume machine-readable data consistently.

Example structured return type idea:

```csharp
public sealed class ForecastSummary
{
    public string PeriodName { get; set; } = string.Empty;
    public int TemperatureF { get; set; }
    public string DetailedForecast { get; set; } = string.Empty;
}
```

Then return `Task<ForecastSummary[]>` for predictable shape.

## 6. Configuration

### 6.1 `appsettings.json`

```json
{
  "WeatherApi": {
    "BaseUrl": "https://api.weather.gov",
    "UserAgentProduct": "weather-mcp",
    "UserAgentVersion": "1.0"
  }
}
```

### 6.2 Field-by-field explanation

- `WeatherApi`: configuration section bound to `WeatherApiOptions`.
- `BaseUrl`: base endpoint for outbound weather API calls.
- `UserAgentProduct` and `UserAgentVersion`: sent in HTTP user-agent header.

### 6.3 Wiring config into the server

Key lines already shown in `Program.cs`:

```csharp
builder.Services.Configure<WeatherApiOptions>(
    builder.Configuration.GetSection(WeatherApiOptions.SectionName));
```

and:

```csharp
var options = sp.GetRequiredService<IOptions<WeatherApiOptions>>().Value;
```

This pattern keeps settings externalized and environment-friendly.

## 7. Running the Server

Build and run locally:

```bash
dotnet build
dotnet run
```

What happens when it starts:

- The process waits for MCP messages on stdin.
- It emits responses/events on stdout.
- It does not expose HTTP endpoints by default in stdio mode.

## 8. Testing the MCP Server

### 8.1 Test with MCP Inspector (recommended)

Official MCP Inspector lets you connect to a stdio server and call tools.

- Inspector repo: https://github.com/modelcontextprotocol/inspector

Typical flow:

1. Launch inspector.
2. Configure transport as `stdio`.
3. Set command to run your server, for example: `dotnet run --project /absolute/path/to/weather-mcp.csproj`
4. Click connect.
5. List tools and invoke `GetAlerts` / `GetForecast`.

### 8.2 Example request/response payloads

Example call for alerts:

```json
{
  "method": "tools/call",
  "params": {
    "name": "GetAlerts",
    "arguments": {
      "state": "CA"
    }
  }
}
```

Example response shape (conceptual):

```json
{
  "content": [
    {
      "type": "text",
      "text": "Event: ..."
    }
  ]
}
```

Example call for forecast:

```json
{
  "method": "tools/call",
  "params": {
    "name": "GetForecast",
    "arguments": {
      "latitude": 38.5816,
      "longitude": -121.4944
    }
  }
}
```

### 8.3 Testing from AI clients

Most MCP clients require a local server command and transport type (`stdio`). Once configured, your tools appear in the client tool list and become callable during prompts.

### 8.4 Map This Server to AI Clients (Cursor and Claude Code)

This is the part many teams miss: MCP is not only server code, it is also client wiring. The client needs JSON configuration that tells it how to start your server process.

#### What this JSON actually is

The JSON is a client-side process definition for your MCP server. It is not sent to the weather API. It is read by the AI client app so it knows:

- Which server name to show in UI (`weather-dotnet` for example)
- Which executable to run (`dotnet`)
- Which arguments to pass (`run --project ...` or a `.dll` path)
- Which environment variables to inject

Conceptually:

```json
{
  "mcpServers": {
    "weather-dotnet": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["..."],
      "env": {}
    }
  }
}
```

Field meaning:

- `mcpServers`: dictionary of named servers.
- `weather-dotnet`: your server alias in the client UI.
- `type: "stdio"`: client communicates through stdin/stdout.
- `command`: executable to spawn.
- `args`: command arguments.
- `env`: environment variables available to your server process.

#### Two launch styles: DLL vs CSPROJ

You can run the same .NET MCP server in two common ways.

Option A: Run compiled DLL (recommended for stable/dev-team setups)

```json
{
  "mcpServers": {
    "weather-dotnet": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "/ABSOLUTE/PATH/weather-mcp/bin/Debug/net8.0/weather-mcp.dll"
      ],
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

When to use:

- Faster startup after build.
- Predictable executable artifact.
- Better for checked-in team configs.

Important:

- You must build first: `dotnet build`.
- If DLL path changes (framework/configuration), update config.

Option B: Run from project file (`dotnet run --project ...`)

```json
{
  "mcpServers": {
    "weather-dotnet": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/ABSOLUTE/PATH/weather-mcp/weather-mcp.csproj"
      ],
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

When to use:

- Easiest while iterating on code.
- No manual DLL path management.
- Slower startup because build/run happens on launch.

#### Which file stores this JSON in each client

Claude Code:

- Project-shared config: `.mcp.json` at repo root.
- User/local config: `~/.claude.json`.
- Teams usually commit `.mcp.json` and keep secrets/user-specific entries in `~/.claude.json`.

Claude Desktop (if you also test there):

- macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`

Cursor:

- Project config is typically `.cursor/mcp.json` in repo root.
- Some Cursor setups can also be configured through Cursor Settings UI for user-level servers.
- If your Cursor version differs, verify the exact path in current Cursor MCP docs/settings screen.

#### Do you need to run the app manually first?

Usually no.

With `type: "stdio"`, the AI client launches your server process automatically using `command` + `args` from JSON when needed.

Runtime sequence:

1. You open Cursor or Claude Code.
2. Client reads MCP server config JSON.
3. Client spawns your .NET process (`dotnet ...`).
4. MCP handshake happens over stdin/stdout.
5. Client requests tool list.
6. On prompt, model selects tool and sends tool call.
7. Your method executes and returns result.

When you do need manual run:

- Manual `dotnet run` is useful only for debugging outside the client.
- For normal MCP usage, the client owns process lifecycle.

#### Example prompt-to-tool mapping in real clients

Prompt:

`What is the weather forecast for Sacramento?`

What the model sees from your server metadata:

- Tool: `GetForecast`
- Description: `Get weather forecast for a location.`
- Inputs:
  - `latitude` (number): `Latitude of the location.`
  - `longitude` (number): `Longitude of the location.`

Client/model action:

- Chooses `GetForecast` due to description match.
- Resolves coordinates.
- Sends MCP `tools/call` with those arguments.
- Renders returned text in chat.

That is why tool/parameter descriptions are critical: they drive tool selection quality.

## 9. Best Practices

### Error handling

- Throw meaningful exceptions for invalid upstream responses.
- Validate user-facing inputs early (state code shape, coordinate range).
- Return concise, readable error text for LLM usability.

### Logging

- Add structured logging (`ILogger`) around external calls and failures.
- Avoid logging secrets/tokens.
- Include correlation context where available.

### Tool design and structure

- Keep each tool narrowly scoped and composable.
- Use clear names and rich `Description` attributes.
- Prefer strongly typed outputs for workflows that chain tools.
- Keep network access in dedicated helpers/clients.

### Reliability and performance

- Reuse `HttpClient` via DI singleton or factory.
- Add timeouts/retries for external APIs where appropriate.
- Cache deterministic responses if latency/cost matters.

## 10. Conclusion

You built a complete MCP server in .NET with:

- MCP runtime setup and stdio transport
- Attribute-based tool registration
- DI-backed service injection
- Schema-friendly parameter descriptions
- Local testing using MCP Inspector

Good extensions to build next:

1. Add domain tools (tickets, repos, CI, observability).
2. Return structured DTOs instead of plain text.
3. Add validation, retries, and richer diagnostics.
4. Split tools into separate assemblies/modules for large systems.

Further reading:

- MCP docs: https://modelcontextprotocol.io/
- MCP specification: https://spec.modelcontextprotocol.io/
- .NET dependency injection: https://learn.microsoft.com/dotnet/core/extensions/dependency-injection
- .NET options/configuration: https://learn.microsoft.com/dotnet/core/extensions/options
