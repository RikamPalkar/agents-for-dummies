using ModelContextProtocol.Server;
using ShellMcpServer.Models;
using ShellMcpServer.Services;
using System.ComponentModel;

namespace ShellMcpServer.Tools;

[McpServerToolType]
public sealed class CommandTools(ICommandExecutionService commandExecutionService)
{
    [McpServerTool(Name = "execute_command"), Description("Executes a local process without invoking a shell. Pass the executable as 'command' and each argument as a separate array element.")]
    public Task<CommandExecutionResult> ExecuteCommandAsync(
        [Description("Executable or command name to run.")] string command,
        [Description("Arguments passed directly to the process. Do not include shell quoting.")] string[]? arguments = null,
        [Description("Optional working directory. Defaults to the server process working directory.")] string? workingDirectory = null,
        [Description("Optional timeout in seconds. Defaults to the configured server timeout.")] int? timeoutSeconds = null,
        CancellationToken cancellationToken = default)
    {
        CommandExecutionRequest request = new(
            command,
            arguments ?? [],
            workingDirectory,
            timeoutSeconds);

        return commandExecutionService.ExecuteAsync(request, cancellationToken);
    }
}