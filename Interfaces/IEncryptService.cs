using Microsoft.Extensions.Primitives;

namespace devault.Interfaces;

public interface IEncryptService
{
    public string Encrypt(string Data);
    public string Decrypt(string EncryptedValue);

    
}
