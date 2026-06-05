using System.Security.Cryptography;

namespace SimpleHashChecker.Core;

public sealed class FileHashService
{
    private const int BufferSize = 1024 * 1024;

    public async Task<string> ComputeHashAsync(
        string filePath,
        HashAlgorithmKind algorithmKind,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        using var algorithm = CreateAlgorithm(algorithmKind);
        var buffer = new byte[BufferSize];
        var totalRead = 0L;

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0)
            {
                break;
            }

            totalRead += bytesRead;
            algorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
            if (stream.Length > 0)
            {
                progress?.Report((double)totalRead / stream.Length);
            }
        }

        algorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        progress?.Report(1);

        return Convert.ToHexString(algorithm.Hash ?? Array.Empty<byte>()).ToLowerInvariant();
    }

    public static bool HashesMatch(string expectedHash, string actualHash)
    {
        var normalizedExpected = NormalizeHash(expectedHash);
        var normalizedActual = NormalizeHash(actualHash);

        return normalizedExpected.Length > 0
            && normalizedExpected.Equals(normalizedActual, StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeHash(string value)
    {
        return new string(value.Where(Uri.IsHexDigit).ToArray()).ToLowerInvariant();
    }

    private static HashAlgorithm CreateAlgorithm(HashAlgorithmKind algorithmKind)
    {
        return algorithmKind switch
        {
            HashAlgorithmKind.MD5 => MD5.Create(),
            HashAlgorithmKind.SHA1 => SHA1.Create(),
            HashAlgorithmKind.SHA256 => SHA256.Create(),
            HashAlgorithmKind.SHA512 => SHA512.Create(),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithmKind), algorithmKind, null)
        };
    }
}
