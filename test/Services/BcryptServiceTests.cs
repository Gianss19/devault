using devault.Services;

namespace test.Services;

public class BcryptServiceTests
{
    private readonly BcryptService _bcryptService = new();

    [Fact]
    public void GenerateHash_ReturnsValidHash()
    {
        var hash = _bcryptService.GenerateHash("Test@1234");

        Assert.False(string.IsNullOrEmpty(hash));
        Assert.StartsWith("$2", hash);
    }

    [Fact]
    public void GenerateHash_WeakPasswordNoUppercase_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => _bcryptService.GenerateHash("test@1234"));
    }

    [Fact]
    public void GenerateHash_WeakPasswordNoDigit_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => _bcryptService.GenerateHash("Test@abcd"));
    }

    [Fact]
    public void GenerateHash_WeakPasswordNoSpecialChar_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => _bcryptService.GenerateHash("Test12345"));
    }

    [Fact]
    public void GenerateHash_WeakPasswordTooShort_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => _bcryptService.GenerateHash("T@1a"));
    }

    [Fact]
    public void IsPasswordValid_ReturnsTrueForCorrectPassword()
    {
        var hash = _bcryptService.GenerateHash("Test@1234");

        var result = _bcryptService.IsPasswordValid("Test@1234", hash);

        Assert.True(result);
    }

    [Fact]
    public void IsPasswordValid_ReturnsFalseForWrongPassword()
    {
        var hash = _bcryptService.GenerateHash("Test@1234");

        var result = _bcryptService.IsPasswordValid("Wrong@1234", hash);

        Assert.False(result);
    }
}
