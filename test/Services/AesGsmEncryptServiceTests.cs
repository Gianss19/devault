using devault.Data.Configuration;
using devault.Services;
using Microsoft.Extensions.Options;

namespace test.Services;

public class AesGsmEncryptServiceTests
{
    private readonly AesGsmEncryptService _encryptService;

    public AesGsmEncryptServiceTests()
    {
        _encryptService = new AesGsmEncryptService(
            Options.Create(new CryptoSettings { MasterKey = "test-master-key-for-encryption" }));
    }

    [Fact]
    public void Encrypt_ReturnsNonEmptyBase64String()
    {
        var result = _encryptService.Encrypt("hello world");

        Assert.False(string.IsNullOrEmpty(result));
        Assert.DoesNotContain(" ", result);
    }

    [Fact]
    public void Decrypt_OfEncryptedData_ReturnsOriginalData()
    {
        var original = "hello world";

        var encrypted = _encryptService.Encrypt(original);
        var decrypted = _encryptService.Decrypt(encrypted);

        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void DifferentEncryptions_ProduceDifferentCiphertexts()
    {
        var data = "hello world";

        var encrypted1 = _encryptService.Encrypt(data);
        var encrypted2 = _encryptService.Encrypt(data);

        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void Constructor_ThrowsWhenMasterKeyIsEmpty()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            new AesGsmEncryptService(Options.Create(new CryptoSettings { MasterKey = "" })));
    }
}
