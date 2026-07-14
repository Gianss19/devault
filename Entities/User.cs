namespace devalut.Entities;

using System.Text.RegularExpressions;

using devalut;


public class User
{
    public Guid Id {get; private set;}
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash {get; private set;}

    public DateTime CreatedAt {get; private set;}
    public DateTime? UpdatedAt {get; set;}
    public List<Secret> Secrets {get; private set;}
    private User()
    {
        
    }
    public User(string name, string email, string passwordHash)
    
    {
        if(string.IsNullOrEmpty(name))
            throw new EntityException("Nombre vacío.");

        if(name.Length < 3 || name.Length > 100)
            throw new EntityException("Longitud del nombre inválida.");
        
        if(email.Length < 3 || email.Length > 100 )
            throw new EntityException("el correo debe tener al menos 3 caracteres.");
       
        if(!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            throw new EntityException("Formato del Email inválido.");
            
        if(passwordHash.Length != 60)
            throw new EntityException("Longitud del hash inválda.");

        Id = new Guid();
        Name = name.Trim();
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    public void ChangeName(string newName)
    {
        if(string.IsNullOrEmpty(newName))
            throw new EntityException("Nombre vacío.");

        if(newName.Length < 3 || newName.Length > 100)
            throw new EntityException("Longitud del nombre inválida.");

        Name = newName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
    
}
