using System.Diagnostics;

namespace ShellMcpServer.Services;

public interface IProcessRunner
{
    Task<ProcessRunResult> RunAsync(
        ProcessStartInfo startInfo,
        TimeSpan timeout,
        int maxOutputCharacters,
        CancellationToken cancellationToken);
}