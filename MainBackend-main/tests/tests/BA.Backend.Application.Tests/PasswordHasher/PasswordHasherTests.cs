using BA.Backend.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace BA.Backend.Application.Tests.PasswordHasher;

public class PasswordHasherTests
{
    private readonly Infrastructure.Services.PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        var mockLogger = new Mock<ILogger<Infrastructure.Services.PasswordHasher>>();
        _passwordHasher = new Infrastructure.Services.PasswordHasher(mockLogger.Object);
    }

    [Fact]
    public void Hash_WithValidPassword_ShouldReturnHashedPassword()
    {
        var pwd = "TestPassword123!";

        var hash = _passwordHasher.Hash(pwd);

        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(pwd);
        hash.StartsWith("$2a$").Should().BeTrue();
    }

    [Fact]
    public void Hash_WithSamePassword_ShouldReturnDifferentHashes()
    {
        var pwd = "TestPassword123!";

        var hash1 = _passwordHasher.Hash(pwd);
        var hash2 = _passwordHasher.Hash(pwd);

        hash1.Should().NotBe(hash2);
    }

    [Theory]
    [InlineData("Simple123")]
    [InlineData("Complex!@#$%^&*()")]
    [InlineData("12345678")]
    [InlineData("VeryLongPasswordWithManyCharactersThatShouldWork")]
    public void Hash_WithVariousPasswords_ShouldReturnValidHashes(string pwd)
    {
        var hash = _passwordHasher.Hash(pwd);

        hash.Should().NotBeNullOrEmpty();
        hash.Length.Should().BeGreaterThan(50);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        var pwd = "CorrectPassword123!";
        var hash = _passwordHasher.Hash(pwd);

        var result = _passwordHasher.Verify(pwd, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        var pwd = "CorrectPassword123!";
        var wrongPwd = "WrongPassword456!";
        var hash = _passwordHasher.Hash(pwd);

        var result = _passwordHasher.Verify(wrongPwd, hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithInvalidHashFormat_ShouldReturnFalse()
    {
        var pwd = "TestPassword123!";
        var invalidHash = "invalid-hash-format";

        var result = _passwordHasher.Verify(pwd, invalidHash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyPassword_ShouldReturnFalse()
    {
        var pwd = "TestPassword123!";
        var hash = _passwordHasher.Hash(pwd);

        var result = _passwordHasher.Verify("", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyHash_ShouldReturnFalse()
    {
        var pwd = "TestPassword123!";

        var result = _passwordHasher.Verify(pwd, "");

        result.Should().BeFalse();
    }
}
