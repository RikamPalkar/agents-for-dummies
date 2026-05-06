using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace ShellMcpServer.Services;

public sealed class ProcessRunner : IProcessRunner
{
    public async Task<ProcessRunResult> RunAsync(
        ProcessStartInfo startInfo,
        TimeSpan timeout,
        int maxOutputCharacters,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(startInfo);

        using Process process = new()
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true,
        };

        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException($"Failed to start process '{startInfo.FileName}'.");
            }

            Task<CapturedText> stdoutTask = ReadToLimitAsync(process.StandardOutput, maxOutputCharacters);
            Task<CapturedText> stderrTask = ReadToLimitAsync(process.StandardError, maxOutputCharacters);

            bool timedOut = await WaitForExitAsync(process, timeout, cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();

            CapturedText stdout = await stdoutTask.ConfigureAwait(false);
            CapturedText stderr = await stderrTask.ConfigureAwait(false);

            return new ProcessRunResult(
                process.ExitCode,
                stdout.Text,
                stderr.Text,
                timedOut,
                stdout.Truncated,
                stderr.Truncated,
                stopwatch.ElapsedMilliseconds);
        }
        catch
        {
            stopwatch.Stop();
            TryKill(process);
            throw;
        }
    }

    private static async Task<bool> WaitForExitAsync(Process process, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeoutSource = new(timeout);
        using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutSource.Token);

        try
        {
            await process.WaitForExitAsync(linkedSource.Token).ConfigureAwait(false);
            return false;
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            TryKill(process);
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            return true;
        }
        catch
        {
            TryKill(process);
            throw;
        }
    }

    private static async Task<CapturedText> ReadToLimitAsync(StreamReader reader, int maxCharacters)
    {
        char[] buffer = ArrayPool<char>.Shared.Rent(1024);

        try
        {
            StringBuilder builder = new(Math.Min(maxCharacters, 4096));
            int totalCharacters = 0;
            bool truncated = false;

            while (true)
            {
                int read = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length)).ConfigureAwait(false);
                if (read == 0)
                {
                    break;
                }

                if (totalCharacters >= maxCharacters)
                {
                    truncated = true;
                    continue;
                }

                int charactersToCopy = Math.Min(read, maxCharacters - totalCharacters);
                builder.Append(buffer, 0, charactersToCopy);
                totalCharacters += charactersToCopy;

                if (charactersToCopy < read)
                {
                    truncated = true;
                }
            }

            return new CapturedText(builder.ToString(), truncated);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    private sealed record CapturedText(string Text, bool Truncated);
}