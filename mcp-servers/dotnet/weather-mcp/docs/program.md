# Program.cs Walkthrough

## Overview

This file is the **entry point** of the MCP (Model Context Protocol) server. It sets up and runs a hosted .NET application that exposes tools over standard input/output (stdio).

> **C# concept — Entry point**
> Every C# program starts from one place. In modern .NET (C# 9+), you can write top-level statements directly in a file without wrapping everything in a `class` and `static void Main()`. The compiler treats this file as the entry point automatically. Older C# code required an explicit `static void Main(string[] args)` method.

---

## Line-by-Line Explanation

```csharp
using Microsoft.Extensions.DependencyInjection;
```
Brings in the **Dependency Injection (DI)** namespace. This gives you access to `IServiceCollection` — the container where you register services the app needs.

> **C# concept — `using` directives**
> `using` at the top of a file imports a **namespace**. A namespace is just a folder-like grouping of related classes. Without this line, you'd have to write the full name every time: `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions`. The `using` directive lets you write the short name instead.

> **C# concept — Namespaces and NuGet packages**
> Namespaces are logical groupings in code. NuGet packages are physical bundles of compiled code (`.dll` files) you download. One NuGet package can contain many namespaces. You add a package with `dotnet add package`, then use `using` to access its types.

---

```csharp
using Microsoft.Extensions.Hosting;
```
Brings in the **.NET Generic Host** infrastructure. The `Host` class lets you build a long-running application with built-in support for DI, configuration, and logging.

> **C# concept — Generic Host**
> .NET applications (web apps, background services, CLI tools) all share a common hosting model. The host manages:
> - The **lifetime** of the app (start, run, stop)
> - **Dependency Injection** container setup
> - **Configuration** loading (appsettings.json, env vars, etc.)
> - **Logging**
>
> Think of it as the backbone of any modern .NET application.

---

```csharp
using ModelContextProtocol;
```
Brings in the **MCP SDK** namespace. This provides the `AddMcpServer()` extension method and other MCP-specific types.

> **C# concept — Extension methods**
> `AddMcpServer()` is not a method defined on the `IServiceCollection` interface itself. It's an **extension method** — a static method in another class that *looks like* it belongs to the type. This is how .NET libraries add functionality to existing types without modifying them. You'll see this pattern everywhere in .NET (LINQ, ASP.NET Core, etc.).

---

```csharp
using System.Net.Http.Headers;
```
Brings in types for working with HTTP headers — specifically `ProductInfoHeaderValue`, which is used to set the `User-Agent` header on HTTP requests.

> **C# concept — `System` namespace**
> `System` is the root namespace of the .NET Base Class Library (BCL) — the standard library that ships with every .NET installation. `System.Net.Http` contains the networking types (`HttpClient`, `HttpRequestMessage`, etc.). You don't need to install a NuGet package for these; they're always available.

---

```csharp
var builder = Host.CreateEmptyApplicationBuilder(settings: null);
```
Creates a **host builder** — a blank slate for configuring your application. Unlike `Host.CreateDefaultBuilder()`, the "empty" variant skips default configuration sources (like `appsettings.json` and environment variables), giving you full control over what gets added.

> **C# concept — `var` (type inference)**
> `var` tells the compiler to figure out the type from the right-hand side. Here the type is `HostApplicationBuilder`. You could write it explicitly: `HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(...)`. Both are identical — `var` just saves typing and reduces noise.

> **C# concept — Builder pattern**
> The Builder pattern is common in .NET. Instead of constructing a complex object in one step, you:
> 1. Create a *builder* object
> 2. Configure it step by step (add services, set options, etc.)
> 3. Call `.Build()` at the end to get the final object
>
> This separates configuration time from run time and makes the code readable.

> **C# concept — Named parameters**
> `settings: null` uses a **named parameter** — you're explicitly saying "I'm passing `null` for the `settings` parameter." This is optional but improves readability, especially when a method has multiple parameters and you're passing `null` for some.

---

```csharp
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
```

### `builder.Services.AddMcpServer()`
Registers the **MCP server** into the DI container. This sets up all the internal plumbing needed to handle MCP protocol messages.

> **C# concept — `IServiceCollection` and service registration**
> `builder.Services` is of type `IServiceCollection`. It's a list of service descriptors — each entry says "when someone asks for type X, give them instance Y". Calling `AddMcpServer()` adds many entries at once (all the internal MCP classes it needs to function).

### `.WithStdioServerTransport()`
Tells the MCP server to communicate over **standard input/output (stdio)**. This is how AI clients (like Claude Desktop or VS Code Copilot) talk to a locally running MCP server — they launch the process and send JSON messages over stdin/stdout.

> **C# concept — Method chaining / Fluent API**
> Notice how each method returns an object that has more methods on it, so you can chain calls with `.`. This is called a **Fluent API**. The methods return `this` (or a related builder object) so you can keep configuring without creating intermediate variables. ASP.NET Core and LINQ use this style heavily.

> **.NET concept — stdin/stdout**
> Every process on any OS has three standard streams: `stdin` (input), `stdout` (output), `stderr` (errors). In .NET, these are accessible via `Console.In`, `Console.Out`, `Console.Error`. MCP over stdio means the host process writes JSON to this process's stdin, and reads JSON back from stdout.

### `.WithToolsFromAssembly()`
Scans the **current assembly** for classes/methods decorated with MCP attributes and registers them as callable tools automatically.

> **C# concept — Assembly**
> When you build a C# project, the compiler produces a `.dll` (or `.exe`) file — this is called an **assembly**. It contains your compiled code plus metadata (type names, method signatures, attributes). An assembly is the unit of deployment in .NET.

> **C# concept — Reflection and attributes**
> `[McpServerTool]` is an **attribute** — metadata you attach to a class or method using square brackets. At runtime, code can use **reflection** (`System.Reflection`) to inspect an assembly, find all types/methods that have a specific attribute, and act on them. This is exactly what `WithToolsFromAssembly()` does — it reflects over your assembly to auto-discover tools.

---

```csharp
builder.Services.AddSingleton(_ =>
{
    var client = new HttpClient() { BaseAddress = new Uri("https://api.weather.gov") };
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weather-tool", "1.0"));
    return client;
});
```
Registers a **pre-configured `HttpClient`** as a singleton service.

> **C# concept — Lambda expressions**
> `_ =>  { ... }` is a **lambda** — an anonymous (inline) function. The `_` is the parameter (an `IServiceProvider` the DI container passes in, but it's not needed here, so it's named `_` by convention to signal "I'm ignoring this"). The body creates and returns an `HttpClient`. Lambdas are used everywhere in C#: LINQ, event handlers, callbacks, DI factory registrations like this one.

> **C# concept — Object initializer syntax**
> `new HttpClient() { BaseAddress = new Uri("...") }` uses an **object initializer**. Instead of:
> ```csharp
> var client = new HttpClient();
> client.BaseAddress = new Uri("...");
> ```
> You can set properties inline in `{ }` right after `new`. Both are equivalent — the initializer is just more concise.

> **C# concept — `Uri` class**
> URLs in .NET are not plain strings for APIs that need them — they're `Uri` objects. `Uri` validates the format and provides properties like `.Host`, `.Scheme`, `.PathAndQuery`. `HttpClient.BaseAddress` requires a `Uri`, not a `string`.

> **.NET concept — `HttpClient` and socket exhaustion**
> `HttpClient` is .NET's built-in HTTP client. Even though you call `new HttpClient()`, the actual TCP connections are managed by `SocketsHttpHandler` underneath — a connection pool shared by requests to the same host. 
>
> **Why singleton matters here:** If you created `new HttpClient()` inside every tool call (transient), each instance gets its own socket pool. Disposing `HttpClient` doesn't immediately release sockets — the OS holds them in a TIME_WAIT state for ~2 minutes. Under any load, you'd rapidly run out of available sockets (socket exhaustion). A singleton means one pool, connections are reused, and the OS is never overwhelmed.

> **C# concept — `AddSingleton` with a factory**
> `AddSingleton(factory)` lets you provide a function that creates the service instead of just a type. This is useful when the object needs custom setup (like setting `BaseAddress` and `UserAgent`) before being stored. The factory runs **once**, and the result is cached and reused for the entire app lifetime.

---

```csharp
var app = builder.Build();
```
**Finalizes** the host configuration and builds the app.

> **C# concept — Build phase vs. run phase**
> .NET separates *configuring* an app from *running* it. Everything before `.Build()` is the **configuration phase** — you're just describing what you want. `.Build()` is the moment the DI container is constructed, all registrations are validated, and the app object is created. After this line, you cannot register new services.

---

```csharp
await app.RunAsync();
```
**Starts** the host and keeps it running until a shutdown signal is received.

> **C# concept — `async`/`await`**
> `await` pauses execution of the current method until the awaited task completes, *without blocking the thread*. This is C#'s **asynchronous programming model**. Instead of freezing the thread while waiting for I/O (network, disk), the thread is freed to do other work. `RunAsync()` returns a `Task` that only completes when the app shuts down — so `await` here means "keep the program alive until shutdown."

> **C# concept — Top-level `await`**
> In older C#, you couldn't use `await` at the top level — it had to be inside an `async` method. In C# 9+, the compiler wraps the whole file in an implicit `async Task Main()`, so `await` works directly in top-level statements.

> **.NET concept — Graceful shutdown**
> When you press `Ctrl+C`, .NET sends a `CancellationToken` cancellation signal through the host. All hosted services get a chance to stop cleanly (finish in-flight work, close connections) before the process exits. This is called **graceful shutdown** and is built into the Generic Host automatically.

---

## What is `builder.Services`? (Deep Dive)

`builder.Services` is an `IServiceCollection` — the **Dependency Injection (DI) container** for your application.

### What is Dependency Injection?

Without DI, a class creates its own dependencies:
```csharp
// Without DI — tightly coupled, hard to test
public class WeatherTools
{
    private HttpClient _client = new HttpClient(); // creates its own
}
```

With DI, a class *declares* what it needs and the container provides it:
```csharp
// With DI — loosely coupled, easy to test
public class WeatherTools
{
    private readonly HttpClient _client;

    public WeatherTools(HttpClient client) // container injects this
    {
        _client = client;
    }
}
```

> **C# concept — Constructor injection**
> The most common DI pattern in C#. You declare dependencies as constructor parameters. The DI container sees "to create `WeatherTools`, I need an `HttpClient`" — looks it up in the registry — and passes it in automatically. You never call `new WeatherTools(...)` yourself.

> **C# concept — `readonly` fields**
> `private readonly HttpClient _client` means the field can only be assigned once (in the constructor) and never changed. This is a best practice for injected dependencies — it signals that this is an immutable reference, preventing accidental reassignment.

> **C# concept — Interfaces and `IServiceCollection`**
> `IServiceCollection` starts with `I` — the C# convention for **interfaces**. An interface defines a contract (what methods/properties exist) without implementing them. This means you can swap out the real implementation with a fake one in tests. Almost all .NET abstractions are interfaces for this reason.

### Service Lifetimes

| Lifetime | Method | Created | Destroyed | Use for |
|---|---|---|---|---|
| **Singleton** | `AddSingleton<T>()` | Once, on first request | When app shuts down | Shared resources: `HttpClient`, caches, config |
| **Scoped** | `AddScoped<T>()` | Once per scope/request | When scope ends | Per-request state: DB contexts, unit-of-work |
| **Transient** | `AddTransient<T>()` | Every time requested | When consumer is done | Lightweight, stateless services |

> **C# concept — Generics (`<T>`)**
> `AddSingleton<T>()` uses a **generic type parameter** `T`. Generics let you write one method that works for any type. `AddSingleton<HttpClient>()` registers `HttpClient`. `AddSingleton<MyService>()` registers `MyService`. The `<T>` is replaced at compile time with the actual type — no casting or boxing needed.

### How it all connects

```
Program.cs (startup)
  builder.Services.AddSingleton<HttpClient>(...)  ← register
  builder.Services.AddMcpServer()                 ← register MCP internals

app.RunAsync() (runtime)
  MCP framework receives a tool call
    → needs to create WeatherTools
    → sees constructor needs HttpClient
    → looks up HttpClient in DI container
    → finds the singleton instance
    → passes it to WeatherTools constructor
    → calls the tool method
```

Everything is wired together automatically. You never call `new WeatherTools(new HttpClient(...))` — the container handles all of it.
