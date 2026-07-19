namespace devault.Models;

public class Secret
{
    public Guid Id{get; private set;}
    public string Name { get; private set;}
    public string EncryptedValue { get; private set; }

    public Guid UserId{get; private set;}

    public User User{get; set;} = null!;

    public DateTime CreatedAt {get; private set;}

    public DateTime? UpdatedAt {get; set;}


    public Secret(string name, string encryptedValue, Guid userId)
    {
        if(name.Length < 3 || name.Length > 100)
            throw new EntityException("Longitud del nombre del secreto inválida.");
        
        if(encryptedValue.Length < 3 )
            throw new EntityException("Longitud del EncryptedValue inválida.");
        if(userId == Guid.Empty)    
            throw new EntityException("UserId Inválido.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        EncryptedValue = encryptedValue.Trim();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new EntityException("Nombre inválido.");
        name = name.Trim();
        if (name.Length < 3 || name.Length > 100)
            throw new EntityException("El nombre debe tener entre 3 y 100 caracteres.");
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeEncryptedValue(string encryptedValue)
    {
        if (string.IsNullOrWhiteSpace(encryptedValue))
            throw new EntityException("Valor cifrado inválido.");
        EncryptedValue = encryptedValue.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

}
