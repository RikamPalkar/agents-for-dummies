using Microsoft.Extensions.Options;
using ShellMcpServer.Configuration;
using ShellMcpServer.Models;
using System.Diagnostics;

namespace ShellMcpServer.Services;

public sealed class CommandExecutionService : ICommandExecutionService
{
    private readonly ShellMcpServerOptions _options;
    private readonly IProcessRunner _processRunner;

    public CommandExecutionService(IProcessRunner processRunner, IOptions<ShellMcpServerOptions> options)
    {
        _processRunner = processRunner;
        _options = options.Value;
    }

    public async Task<CommandExecutionResult> ExecuteAsync(CommandExecutionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        string workingDirectory = ResolveWorkingDirectory(request.WorkingDirectory);
        int timeoutSeconds = request.TimeoutSeconds ?? _options.DefaultTimeoutSeconds;
        ProcessStartInfo startInfo = CreateStartInfo(request.Command, request.Arguments, workingDirectory);

        ProcessRunResult result = await _processRunner.RunAsync(
            startInfo,
            TimeSpan.FromSeconds(timeoutSeconds),
            _options.MaxOutputCharacters,
            cancellationToken).ConfigureAwait(false);

        return new CommandExecutionResult(
            request.Command,
            request.Arguments,
            workingDirectory,
            result.ExitCode,
            result.StandardOutput,
            result.StandardError,
            result.TimedOut,
            result.StandardOutputTruncated,
            result.StandardErrorTruncated,
            result.DurationMs);
    }

    private void ValidateRequest(CommandExecutionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Command))
        {
            throw new ArgumentException("Command is required.", nameof(request));
        }

        if (request.TimeoutSeconds is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "TimeoutSeconds must be greater than zero when provided.");
        }

        int effectiveTimeout = request.TimeoutSeconds ?? _options.DefaultTimeoutSeconds;
        if (effectiveTimeout > _options.MaxTimeoutSeconds)
        {
            throw new ArgumentOutOfRangeException(nameof(request), $"TimeoutSeconds cannot exceed {_options.MaxTimeoutSeconds}.");
        }

        if (!string.IsNullOrWhiteSpace(request.WorkingDirectory) && !Directory.Exists(request.WorkingDirectory))
        {
            throw new DirectoryNotFoundException($"Working directory '{request.WorkingDirectory}' does not exist.");
        }

        if (_options.AllowedCommands.Length > 0 && !IsAllowedCommand(request.Command))
        {
            throw new InvalidOperationException($"Command '{request.Command}' is not allowed by the current server configuration.");
        }
    }

    private bool IsAllowedCommand(string command)
    {
        string fileName = Path.GetFileName(command);

        return _options.AllowedCommands.Contains(command, StringComparer.OrdinalIgnoreCase)
            || _options.AllowedCommands.Contains(fileName, StringComparer.OrdinalIgnoreCase);
    }

    private static string ResolveWorkingDirectory(string? workingDirectory)
    {
        return string.IsNullOrWhiteSpace(workingDirectory)
            ? Environment.CurrentDirectory
            : Path.GetFullPath(workingDirectory);
    }

    private static ProcessStartInfo CreateStartInfo(string command, IReadOnlyList<string> arguments, string workingDirectory)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = command,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }
}