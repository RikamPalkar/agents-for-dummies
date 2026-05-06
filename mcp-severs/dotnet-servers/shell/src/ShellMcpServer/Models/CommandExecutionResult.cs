namespace ShellMcpServer.Models;

public sealed record CommandExecutionResult(
    string Command,
    IReadOnlyList<string> Arguments,
    string WorkingDirectory,
    int ExitCode,
    string StandardOutput,
    string StandardError,
    bool TimedOut,
    bool StandardOutputTruncated,
    bool StandardErrorTruncated,
    long DurationMs)
{
    public bool Success => !TimedOut && ExitCode == 0;
}