namespace devalut;

public interface IHasherService
{
    
    public Task GenerateHash (string Password);
    public Task Verify(string Password, string PasswordHash);
}
