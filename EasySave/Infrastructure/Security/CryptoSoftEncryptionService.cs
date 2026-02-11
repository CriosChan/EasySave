using System.Diagnostics;
using EasySave.Application.Abstractions;

namespace EasySave.Infrastructure.Security;

/// <summary>
///     CryptoSoft adapter with mono-instance guard.
/// </summary>
public sealed class CryptoSoftEncryptionService : IFileEncryptionService
{
    private static readonly SemaphoreSlim CryptoGate = new(1, 1);
    private readonly IGeneralSettingsStore _settings;

    public CryptoSoftEncryptionService(IGeneralSettingsStore settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public bool ShouldEncrypt(string filePath)
    {
        var ext = Path.GetExtension(filePath)?.ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(ext))
            return false;

        return _settings.Current.CryptoExtensions.Any(x => string.Equals(x, ext, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<long> EncryptAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!ShouldEncrypt(filePath))
            return 0;

        var cfg = _settings.Current;
        if (string.IsNullOrWhiteSpace(cfg.CryptoSoftPath))
            return -10;

        if (!File.Exists(cfg.CryptoSoftPath))
            return -11;

        await CryptoGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var args = string.Format(cfg.CryptoSoftArguments, filePath);
            var sw = Stopwatch.StartNew();
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cfg.CryptoSoftPath,
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                }
            };

            if (!process.Start())
                return -12;

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            sw.Stop();

            if (process.ExitCode != 0)
                return -Math.Abs(process.ExitCode);

            return sw.ElapsedMilliseconds;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return -13;
        }
        finally
        {
            CryptoGate.Release();
        }
    }
}
