using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShellMcpServer.Configuration;
using ShellMcpServer.Services;
using ShellMcpServer.Tools;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
	options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<IValidateOptions<ShellMcpServerOptions>, ShellMcpServerOptionsValidator>();
builder.Services
	.AddOptions<ShellMcpServerOptions>()
	.BindConfiguration(ShellMcpServerOptions.SectionName)
	.ValidateOnStart();

builder.Services.AddSingleton<IProcessRunner, ProcessRunner>();
builder.Services.AddSingleton<ICommandExecutionService, CommandExecutionService>();

builder.Services
	.AddMcpServer()
	.WithStdioServerTransport()
	.WithTools<CommandTools>();

await builder.Build().RunAsync().ConfigureAwait(false);
