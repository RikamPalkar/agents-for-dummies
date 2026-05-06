namespace ShellMcpServer.Services;

public sealed record ProcessRunResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    bool TimedOut,
    bool StandardOutputTruncated,
    bool StandardErrorTruncated,
    long DurationMs);