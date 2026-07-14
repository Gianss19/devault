using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace devalut.Entities;

public class RefreshToken
{
    public Guid Id{get; private set;}
    public Guid UserId{get; private set;}
    public string Token {get; private set;}

    public DateTime CreatedAt{get; private set;}
    public DateTime ExpiresAt{get; private set;}
    public User User{get; private set;} = null!;

    public RefreshToken(Guid userId, string token)
    {
        if(userId == Guid.Empty)
            throw new EntityException("Usuario inválido.");

        if(string.IsNullOrEmpty(token))
            throw new EntityException("Token inválido.");
        
        Id = new Guid();
        UserId = userId;
        Token = token;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddDays(1);


    }


    


}
