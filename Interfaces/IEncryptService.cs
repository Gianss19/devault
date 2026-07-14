namespace devalut.Interfaces;

public interface IEncryptService
{
    public Task Encrypt(string Data);
    public Task Decrypt(string EncryptedValue);

    
}
