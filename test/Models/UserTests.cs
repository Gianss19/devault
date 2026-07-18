using devault;
using devault.Models;
using devault.Models.Enums;

namespace test.Models;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesUserWithCorrectProperties()
    {
        var user = new User("TestUser", "test@example.com", "hashed_password");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("TestUser", user.Name);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("hashed_password", user.PasswordHash);
        Assert.Equal(Roles.User, user.Rol);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
        Assert.Null(user.UpdatedAt);
    }

    [Fact]
    public void Constructor_TrimsWhitespaceAndLowercasesEmail()
    {
        var user = new User("  TestUser  ", "  TEST@Example.COM  ", "hashed_password");

        Assert.Equal("TestUser", user.Name);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public void Constructor_ThrowsWhenNameIsNull()
    {
        Assert.Throws<EntityException>(() => new User(null, "test@example.com", "hashed_password"));
    }

    [Fact]
    public void Constructor_ThrowsWhenNameIsEmpty()
    {
        Assert.Throws<EntityException>(() => new User("", "test@example.com", "hashed_password"));
    }

    [Fact]
    public void Constructor_ThrowsWhenNameIsWhitespace()
    {
        Assert.Throws<EntityException>(() => new User("   ", "test@example.com", "hashed_password"));
    }

    [Fact]
    public void Constructor_ThrowsWhenNameTooShort()
    {
        Assert.Throws<EntityException>(() => new User("ab", "test@example.com", "hashed_password"));
    }

    [Fact]
    public void Constructor_ThrowsWhenNameTooLong()
    {
        var longName = new string('a', 101);
        Assert.Throws<EntityException>(() => new User(longName, "test@example.com", "hashed_password"));
    }

    [Fact]
    public void Constructor_ThrowsWhenEmailIsNull()
    {
        Assert.Throws<EntityException>(() => new User("TestUser", null, "hashed_password"));
    }

    [Fact]
    public void Constructor_ThrowsWhenEmailIsEmpty()
    {
        Assert.Throws<EntityException>(() => new User("TestUser", "", "hashed_password"));
    }

    [Fact]
    public void Constructor_ThrowsWhenEmailIsInvalid()
    {
        Assert.Throws<EntityException>(() => new User("TestUser", "not-an-email", "hashed_password"));
    }

    [Fact]
    public void Constructor_ThrowsWhenPasswordHashIsNull()
    {
        Assert.Throws<EntityException>(() => new User("TestUser", "test@example.com", null));
    }

    [Fact]
    public void Constructor_ThrowsWhenPasswordHashIsEmpty()
    {
        Assert.Throws<EntityException>(() => new User("TestUser", "test@example.com", ""));
    }

    [Fact]
    public void ChangeName_WithValidName_UpdatesNameAndSetsUpdatedAt()
    {
        var user = new User("TestUser", "test@example.com", "hashed_password");

        user.ChangeName("NewName");

        Assert.Equal("NewName", user.Name);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void ChangeName_WithEmptyName_Throws()
    {
        var user = new User("TestUser", "test@example.com", "hashed_password");

        Assert.Throws<EntityException>(() => user.ChangeName(""));
    }

    [Fact]
    public void ChangeName_WithTooShortName_Throws()
    {
        var user = new User("TestUser", "test@example.com", "hashed_password");

        Assert.Throws<EntityException>(() => user.ChangeName("ab"));
    }

    [Fact]
    public void ChangePassword_WithValidHash_UpdatesHashAndSetsUpdatedAt()
    {
        var user = new User("TestUser", "test@example.com", "old_hash");

        user.ChangePassword("new_hash");

        Assert.Equal("new_hash", user.PasswordHash);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void ChangePassword_WithEmptyHash_Throws()
    {
        var user = new User("TestUser", "test@example.com", "old_hash");

        Assert.Throws<EntityException>(() => user.ChangePassword(""));
    }

    [Fact]
    public void ChangeRole_UpdatesRoleAndSetsUpdatedAt()
    {
        var user = new User("TestUser", "test@example.com", "hashed_password");

        user.ChangeRole(Roles.Admin);

        Assert.Equal(Roles.Admin, user.Rol);
        Assert.NotNull(user.UpdatedAt);
    }
}
