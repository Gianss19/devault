namespace devalut;

public interface IHasherService
{
    
    public string GenerateHash (string Password);
    public bool IsPasswordValid(string Password, string PasswordHash);
}
