using devault;
using devault.Models;

namespace test.Models;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_CreatesTokenWithCorrectProperties()
    {
        var userId = Guid.NewGuid();
        var token = new RefreshToken(userId, "test_token_hash", "test_token", TimeSpan.FromDays(7));

        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(userId, token.UserId);
        Assert.Equal("test_token_hash", token.Token);
        Assert.Equal("test_token", token.PlainTextToken);
        Assert.True(token.CreatedAt <= DateTime.UtcNow);
        Assert.True(token.ExpiresAt > token.CreatedAt);
        Assert.Null(token.RevokedAt);
    }

    [Fact]
    public void IsExpired_ReturnsFalseWhenNotExpired()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", "test_token", TimeSpan.FromDays(7));

        Assert.False(token.IsExpired);
    }

    [Fact]
    public void IsExpired_ReturnsTrueWhenExpired()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", "test_token", TimeSpan.Zero);

        Assert.True(token.IsExpired);
    }

    [Fact]
    public void IsRevoked_ReturnsFalseInitially()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", "test_token", TimeSpan.FromDays(7));

        Assert.False(token.IsRevoked);
    }

    [Fact]
    public void IsRevoked_ReturnsTrueAfterRevoke()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", "test_token", TimeSpan.FromDays(7));

        token.Revoke();

        Assert.True(token.IsRevoked);
    }

    [Fact]
    public void IsActive_ReturnsTrueWhenNotExpiredAndNotRevoked()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", "test_token", TimeSpan.FromDays(7));

        Assert.True(token.IsActive);
    }

    [Fact]
    public void IsActive_ReturnsFalseWhenExpired()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", "test_token", TimeSpan.Zero);

        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_ReturnsFalseWhenRevoked()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", "test_token", TimeSpan.FromDays(7));

        token.Revoke();

        Assert.False(token.IsActive);
    }

    [Fact]
    public void Revoke_SetsRevokedAt()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", "test_token", TimeSpan.FromDays(7));

        token.Revoke();

        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public void DoubleRevoke_IsIdempotent()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", "test_token", TimeSpan.FromDays(7));

        token.Revoke();
        var firstRevokedAt = token.RevokedAt;

        token.Revoke();

        Assert.Equal(firstRevokedAt, token.RevokedAt);
    }

    [Fact]
    public void Constructor_ThrowsForEmptyUserId()
    {
        Assert.Throws<EntityException>(() => new RefreshToken(Guid.Empty, "hash", "test_token", TimeSpan.FromDays(7)));
    }

    [Fact]
    public void Constructor_ThrowsForNullToken()
    {
        Assert.Throws<EntityException>(() => new RefreshToken(Guid.NewGuid(), null, "test_token", TimeSpan.FromDays(7)));
    }

    [Fact]
    public void Constructor_ThrowsForEmptyToken()
    {
        Assert.Throws<EntityException>(() => new RefreshToken(Guid.NewGuid(), "", "test_token", TimeSpan.FromDays(7)));
    }
}
