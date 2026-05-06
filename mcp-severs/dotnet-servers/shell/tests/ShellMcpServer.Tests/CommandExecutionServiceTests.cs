using Microsoft.Extensions.Options;
using ShellMcpServer.Configuration;
using ShellMcpServer.Models;
using ShellMcpServer.Services;
using System.Diagnostics;

namespace ShellMcpServer.Tests;

public sealed class CommandExecutionServiceTests
{
    [Fact]
    public async Task ExecuteAsyncUsesRunnerAndReturnsStructuredResult()
    {
        FakeProcessRunner runner = new(_ => new ProcessRunResult(0, "stdout", "stderr", false, false, false, 12));
        CommandExecutionService service = CreateService(runner);

        CommandExecutionResult result = await service.ExecuteAsync(
            new CommandExecutionRequest("echo", ["hello"], null, 5),
            CancellationToken.None);

        Assert.Equal("echo", result.Command);
        Assert.Equal(["hello"], result.Arguments);
        Assert.Equal(Environment.CurrentDirectory, result.WorkingDirectory);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("stdout", result.StandardOutput);
        Assert.Equal("stderr", result.StandardError);
        Assert.True(result.Success);
        Assert.NotNull(runner.LastStartInfo);
        Assert.Equal("echo", runner.LastStartInfo!.FileName);
        Assert.Equal(["hello"], runner.LastStartInfo.ArgumentList.ToArray());
    }

    [Fact]
    public async Task ExecuteAsyncRejectsCommandsOutsideAllowList()
    {
        FakeProcessRunner runner = new(_ => throw new InvalidOperationException("Runner should not be called."));
        CommandExecutionService service = CreateService(runner, new ShellMcpServerOptions
        {
            DefaultTimeoutSeconds = 30,
            MaxTimeoutSeconds = 300,
            MaxOutputCharacters = 1024,
            AllowedCommands = ["dotnet"],
        });

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(new CommandExecutionRequest("git", [], null, null), CancellationToken.None));

        Assert.Contains("not allowed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsyncRejectsMissingWorkingDirectory()
    {
        FakeProcessRunner runner = new(_ => throw new InvalidOperationException("Runner should not be called."));
        CommandExecutionService service = CreateService(runner);

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
            service.ExecuteAsync(
                new CommandExecutionRequest("echo", [], Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")), null),
                CancellationToken.None));
    }

    [Fact]
    public async Task ProcessRunnerExecutesCommandAndCapturesOutput()
    {
        ProcessRunner runner = new();
        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Environment.CurrentDirectory,
        };
        startInfo.ArgumentList.Add("--version");

        ProcessRunResult result = await runner.RunAsync(startInfo, TimeSpan.FromSeconds(10), 4096, CancellationToken.None);

        Assert.False(result.TimedOut);
        Assert.Equal(0, result.ExitCode);
        Assert.False(string.IsNullOrWhiteSpace(result.StandardOutput));
    }

    private static CommandExecutionService CreateService(FakeProcessRunner runner, ShellMcpServerOptions? options = null)
    {
        return new CommandExecutionService(runner, Options.Create(options ?? new ShellMcpServerOptions()));
    }

    private sealed class FakeProcessRunner(Func<ProcessStartInfo, ProcessRunResult> callback) : IProcessRunner
    {
        public ProcessStartInfo? LastStartInfo { get; private set; }

        public Task<ProcessRunResult> RunAsync(
            ProcessStartInfo startInfo,
            TimeSpan timeout,
            int maxOutputCharacters,
            CancellationToken cancellationToken)
        {
            LastStartInfo = startInfo;
            return Task.FromResult(callback(startInfo));
        }
    }
}