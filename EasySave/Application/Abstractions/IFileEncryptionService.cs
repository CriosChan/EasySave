namespace EasySave.Application.Abstractions;

/// <summary>
///     Encrypts files through an external tool (CryptoSoft).
/// </summary>
public interface IFileEncryptionService
{
    bool ShouldEncrypt(string filePath);
    Task<long> EncryptAsync(string filePath, CancellationToken cancellationToken);
}
