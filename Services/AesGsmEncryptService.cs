using System.Security.Cryptography;
using System.Text;
using devault.Data.Configuration;
using devault.Interfaces;
using Microsoft.Extensions.Options;

namespace devault.Services;

public class AesGsmEncryptService : IEncryptService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key;

    public AesGsmEncryptService(IOptions<CryptoSettings> options)
    {
        var MasterKey = options.Value.MasterKey;
        if (string.IsNullOrEmpty(MasterKey))
            throw new KeyNotFoundException("MasterKey no encontrada.");

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(MasterKey));
    }

    public string Encrypt(string Data)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(Data);
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] tag = new byte[TagSize];
        byte[] cipherText = new byte[dataBytes.Length];

        using var aesGcm = new AesGcm(_key, tagSizeInBytes: TagSize);
        aesGcm.Encrypt(nonce, dataBytes, cipherText, tag);

        var finalEncrypt = new byte[NonceSize + TagSize + cipherText.Length];
        Buffer.BlockCopy(nonce, 0, finalEncrypt, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, finalEncrypt, NonceSize, TagSize);
        Buffer.BlockCopy(cipherText, 0, finalEncrypt, NonceSize + TagSize, cipherText.Length);

        return Convert.ToBase64String(finalEncrypt);
    }

    public string Decrypt(string EncryptedValue)
    {
        byte[] byteValue = Convert.FromBase64String(EncryptedValue);

        byte[] nonce = byteValue[..NonceSize];
        byte[] tag = byteValue[NonceSize..(NonceSize + TagSize)];
        byte[] cipherText = byteValue[(NonceSize + TagSize)..];

        byte[] decryptedBytes = new byte[cipherText.Length];
        using var aesGcm = new AesGcm(_key, tagSizeInBytes: TagSize);
        aesGcm.Decrypt(nonce, cipherText, tag, decryptedBytes);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
