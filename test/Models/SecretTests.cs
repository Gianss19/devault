using devault;
using devault.Models;

namespace test.Models;

public class SecretTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesSecret()
    {
        var userId = Guid.NewGuid();
        var secret = new Secret("MySecret", "encrypted_value", userId);

        Assert.NotEqual(Guid.Empty, secret.Id);
        Assert.Equal("MySecret", secret.Name);
        Assert.Equal("encrypted_value", secret.EncryptedValue);
        Assert.Equal(userId, secret.UserId);
        Assert.True(secret.CreatedAt <= DateTime.UtcNow);
        Assert.Null(secret.UpdatedAt);
    }

    [Fact]
    public void Constructor_ThrowsWhenNameTooShort()
    {
        var userId = Guid.NewGuid();
        Assert.Throws<EntityException>(() => new Secret("ab", "encrypted_value", userId));
    }

    [Fact]
    public void Constructor_ThrowsWhenNameTooLong()
    {
        var userId = Guid.NewGuid();
        var longName = new string('a', 101);
        Assert.Throws<EntityException>(() => new Secret(longName, "encrypted_value", userId));
    }

    [Fact]
    public void Constructor_ThrowsWhenEncryptedValueTooShort()
    {
        var userId = Guid.NewGuid();
        Assert.Throws<EntityException>(() => new Secret("MySecret", "ab", userId));
    }

    [Fact]
    public void Constructor_ThrowsWhenUserIdIsEmpty()
    {
        Assert.Throws<EntityException>(() => new Secret("MySecret", "encrypted_value", Guid.Empty));
    }

    [Fact]
    public void Constructor_TrimsNameAndEncryptedValue()
    {
        var userId = Guid.NewGuid();
        var secret = new Secret("  MySecret  ", "  encrypted_value  ", userId);

        Assert.Equal("MySecret", secret.Name);
        Assert.Equal("encrypted_value", secret.EncryptedValue);
    }
}
