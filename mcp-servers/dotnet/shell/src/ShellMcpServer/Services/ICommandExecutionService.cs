using ShellMcpServer.Models;

namespace ShellMcpServer.Services;

public interface ICommandExecutionService
{
    Task<CommandExecutionResult> ExecuteAsync(CommandExecutionRequest request, CancellationToken cancellationToken);
}