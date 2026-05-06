namespace ShellMcpServer.Configuration;

public sealed class ShellMcpServerOptions
{
    public const string SectionName = "ShellMcpServer";

    public int DefaultTimeoutSeconds { get; init; } = 30;

    public int MaxTimeoutSeconds { get; init; } = 300;

    public int MaxOutputCharacters { get; init; } = 32_768;

    public string[] AllowedCommands { get; init; } = [];
}