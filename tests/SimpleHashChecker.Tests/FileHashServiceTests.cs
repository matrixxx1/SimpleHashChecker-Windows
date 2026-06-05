using SimpleHashChecker.Core;

namespace SimpleHashChecker.Tests;

public sealed class FileHashServiceTests
{
    [Theory]
    [InlineData(HashAlgorithmKind.MD5, "900150983cd24fb0d6963f7d28e17f72")]
    [InlineData(HashAlgorithmKind.SHA1, "a9993e364706816aba3e25717850c26c9cd0d89d")]
    [InlineData(HashAlgorithmKind.SHA256, "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad")]
    [InlineData(HashAlgorithmKind.SHA512, "ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f")]
    public async Task ComputeHashAsync_ReturnsExpectedDigest(HashAlgorithmKind kind, string expected)
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "abc");

        try
        {
            var service = new FileHashService();
            var actual = await service.ComputeHashAsync(path, kind);

            Assert.Equal(expected, actual);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void HashesMatch_IgnoresWhitespaceAndSeparators()
    {
        Assert.True(FileHashService.HashesMatch("90 01-50:98", "90015098"));
    }
}
