using Microsoft.Extensions.Options;

namespace ShellMcpServer.Configuration;

public sealed class ShellMcpServerOptionsValidator : IValidateOptions<ShellMcpServerOptions>
{
    public ValidateOptionsResult Validate(string? name, ShellMcpServerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.DefaultTimeoutSeconds <= 0)
        {
            return ValidateOptionsResult.Fail("DefaultTimeoutSeconds must be greater than zero.");
        }

        if (options.MaxTimeoutSeconds <= 0)
        {
            return ValidateOptionsResult.Fail("MaxTimeoutSeconds must be greater than zero.");
        }

        if (options.DefaultTimeoutSeconds > options.MaxTimeoutSeconds)
        {
            return ValidateOptionsResult.Fail("DefaultTimeoutSeconds cannot exceed MaxTimeoutSeconds.");
        }

        if (options.MaxOutputCharacters <= 0)
        {
            return ValidateOptionsResult.Fail("MaxOutputCharacters must be greater than zero.");
        }

        if (options.AllowedCommands.Any(string.IsNullOrWhiteSpace))
        {
            return ValidateOptionsResult.Fail("AllowedCommands cannot contain empty values.");
        }

        return ValidateOptionsResult.Success;
    }
}