# Shell MCP Server

Shell MCP Server is a .NET 8 stdio-based MCP server that exposes a single tool, `execute_command`, for running local processes.

## Features

- Uses the official `ModelContextProtocol` C# SDK.
- Executes commands without shell expansion by default.
- Enforces configurable timeouts and output-size limits.
- Supports an optional allowlist for approved executables.
- Includes unit tests for validation and command execution behavior.

## Project structure

- `src/ShellMcpServer`: MCP server implementation.
- `tests/ShellMcpServer.Tests`: tests for command execution behavior.

## Configuration

Configuration is loaded from `appsettings.json` and standard .NET configuration providers.

```json
{
  "ShellMcpServer": {
    "DefaultTimeoutSeconds": 30,
    "MaxTimeoutSeconds": 300,
    "MaxOutputCharacters": 32768,
    "AllowedCommands": []
  }
}
```

- `DefaultTimeoutSeconds`: applied when a tool call does not specify a timeout.
- `MaxTimeoutSeconds`: upper bound accepted from callers.
- `MaxOutputCharacters`: maximum number of characters captured from stdout and stderr independently.
- `AllowedCommands`: when non-empty, only these executables can be launched.

## Running locally

```bash
dotnet build ShellMcpServer.sln
dotnet run --project src/ShellMcpServer
```

The server communicates over stdio, so it is intended to be started by an MCP client.

## MCP tool

`execute_command`

- `command`: executable or command name.
- `arguments`: array of raw process arguments.
- `workingDirectory`: optional working directory.
- `timeoutSeconds`: optional timeout for that invocation.

If you need shell semantics such as pipes or redirection, invoke a shell explicitly, for example `/bin/zsh` with `-lc`, rather than relying on implicit shell execution.