namespace ShellMcpServer.Models;

public sealed record CommandExecutionRequest(
    string Command,
    IReadOnlyList<string> Arguments,
    string? WorkingDirectory,
    int? TimeoutSeconds);