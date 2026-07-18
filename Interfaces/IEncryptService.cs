using Microsoft.Extensions.Primitives;

namespace devalut.Interfaces;

public interface IEncryptService
{
    public string Encrypt(string Data);
    public string Decrypt(string EncryptedValue);

    
}
