using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using devalut.Data.Configuration;
using devalut.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;

namespace devalut.Services;

public class AesGsmEncryptService : IEncryptService
{
    private readonly byte[] _key;
    public AesGsmEncryptService(IOptions<CryptoSettings> options)
    {
        var MasterKey = options.Value.MasterKey;
        if(string.IsNullOrEmpty(MasterKey))
            throw new KeyNotFoundException ("MasterKey no encontrada.");

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(MasterKey));    
        
    }

    public string Encrypt(string Data)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(Data);
        
        byte[] nonce = RandomNumberGenerator.GetBytes(16);
        byte[] tag = new byte[16];
        byte[] cipherText = new byte[dataBytes.Length];

        using var aesGsm = new AesGcm(_key, tagSizeInBytes: 16);
        aesGsm.Encrypt(nonce, dataBytes, cipherText, tag);

        var finalEncrypt = new List<byte>();
        finalEncrypt.AddRange(nonce);
        finalEncrypt.AddRange(tag);
        finalEncrypt.AddRange(cipherText);

        var EncryptedText = Convert.ToBase64String(finalEncrypt.ToArray());

        return EncryptedText;
        

    }
 
    public string Decrypt(string EncryptedValue)
    {   
        byte[] byteValue = Convert.FromBase64String(EncryptedValue);

        byte[] nonce = byteValue[..12];
        byte[] tag = byteValue[12..28];
        byte[] cipherText = byteValue[28..];

        byte[] decryptedBytes = new byte[cipherText.Length];
        using var aesGcm = new AesGcm(_key, tagSizeInBytes: 16);

        aesGcm.Decrypt(nonce, cipherText, tag, decryptedBytes);    

        return Encoding.UTF8.GetString(decryptedBytes);
        
    }



}
