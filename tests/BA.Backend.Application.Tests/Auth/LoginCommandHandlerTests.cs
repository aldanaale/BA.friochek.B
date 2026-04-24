using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Auth.Handlers;
using BA.Backend.Application.Exceptions;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Enums;
using BA.Backend.Domain.Models;
using BA.Backend.Domain.Repositories;
using BA.Backend.Application.Common.Interfaces;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace BA.Backend.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<ITenantRepository>        _mockTenantRepo;
    private readonly Mock<IUserRepository>          _mockUserRepo;
    private readonly Mock<IAuthRepository>          _mockAuthRepo;
    private readonly Mock<IUserSessionRepository>   _mockSessionRepo;
    private readonly Mock<ISessionService>          _mockSessionService;
    private readonly Mock<IJwtTokenService>         _mockJwtService;
    private readonly Mock<IPasswordHasher>          _mockHasher;
    private readonly Mock<ILogger<LoginCommandHandler>> _mockLogger;
    private readonly LoginCommandHandler            _handler;

    public LoginCommandHandlerTests()
    {
        _mockTenantRepo     = new Mock<ITenantRepository>();
        _mockUserRepo       = new Mock<IUserRepository>();
        _mockAuthRepo       = new Mock<IAuthRepository>();
        _mockSessionRepo    = new Mock<IUserSessionRepository>();
        _mockSessionService = new Mock<ISessionService>();
        _mockJwtService     = new Mock<IJwtTokenService>();
        _mockHasher         = new Mock<IPasswordHasher>();
        _mockLogger         = new Mock<ILogger<LoginCommandHandler>>();

        _handler = new LoginCommandHandler(
            _mockTenantRepo.Object,
            _mockUserRepo.Object,
            _mockAuthRepo.Object,
            _mockSessionRepo.Object,
            _mockSessionService.Object,
            _mockJwtService.Object,
            _mockHasher.Object,
            _mockLogger.Object);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private const string SeedPwd = "Admin123!";

    private static Tenant MakeTenant(Guid id) =>
        new() { Id = id, Name = "Admin Tenant", Slug = "admin", IsActive = true };

    private static User MakeUser(Guid id, Guid tenantId, bool isActive = true) =>
        new()
        {
            Id           = id,
            TenantId     = tenantId,
            Email        = "admin@test.com",
            PasswordHash = "$2a$12$ufgNR3HGmE7BZXXS7TvnIe",
            Name         = "Admin",
            LastName     = "User",
            Role         = UserRole.Admin,
            IsActive     = isActive
        };

    private static LoginCommand MakeCommand(string password = SeedPwd) =>
        new(Email: "admin@test.com", Password: password, TenantSlug: "admin", DeviceFingerprint: "fp-abc-123");

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var tenantId  = Guid.NewGuid();
        var userId    = Guid.NewGuid();
        var token     = "eyJhbGciOiJIUzI1NiIs...";
        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        var tenant    = MakeTenant(tenantId);
        var user      = MakeUser(userId, tenantId);
        var command   = new LoginCommand(Email: "admin@test.com", Password: "Admin123!", TenantSlug: "admin", DeviceFingerprint: "fp-abc-123");

        var loginResult = new LoginResult { User = user, Tenant = tenant, ActiveSessions = new List<UserSession>() };

        _mockAuthRepo
            .Setup(x => x.GetLoginDataAsync("admin@test.com", "admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        _mockHasher
            .Setup(x => x.Verify("Admin123!", user.PasswordHash))
            .Returns(true);

        _mockJwtService
            .Setup(x => x.GenerateToken(user, tenantId, It.IsAny<string>()))
            .Returns((token, expiresAt));

        _mockSessionRepo
            .Setup(x => x.CreateSessionAsync(It.IsAny<UserSession>()))
            .ReturnsAsync(new UserSession { Id = Guid.NewGuid() });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(token);
        result.UserId.Should().Be(userId);
        result.TenantId.Should().Be(tenantId);
        result.UserFullName.Should().Be("Admin User");
        result.ExpiresAt.Should().Be(expiresAt);

        _mockAuthRepo.Verify(x => x.GetLoginDataAsync("admin@test.com", "admin", It.IsAny<CancellationToken>()), Times.Once);
        _mockHasher.Verify(x => x.Verify("Admin123!", user.PasswordHash), Times.Once);
        _mockJwtService.Verify(x => x.GenerateToken(user, tenantId, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidTenantOrUser_ShouldThrowInvalidCredentialsException()
    {
        var command = new LoginCommand(Email: "admin@test.com", Password: "Admin123!", TenantSlug: "invalid-slug", DeviceFingerprint: "fp-abc-123");

        _mockAuthRepo
            .Setup(x => x.GetLoginDataAsync("admin@test.com", "invalid-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResult { User = null, Tenant = null });

        var ex = await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.InnerException!.Message.Should().Contain("inválidas");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowInvalidCredentialsException()
    {
        var tenantId = Guid.NewGuid();
        var userId   = Guid.NewGuid();
        var tenant   = MakeTenant(tenantId);
        var user     = MakeUser(userId, tenantId);
        var command  = new LoginCommand(Email: "admin@test.com", Password: "WrongPassword!", TenantSlug: "admin", DeviceFingerprint: "fp-abc-123");

        var loginResult = new LoginResult { User = user, Tenant = tenant, ActiveSessions = new List<UserSession>() };

        _mockAuthRepo
            .Setup(x => x.GetLoginDataAsync("admin@test.com", "admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        _mockHasher
            .Setup(x => x.Verify("WrongPassword!", user.PasswordHash))
            .Returns(false);

        var ex = await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.InnerException!.Message.Should().Contain("incorrecta");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldThrowInvalidCredentialsException()
    {
        var tenantId = Guid.NewGuid();
        var userId   = Guid.NewGuid();
        var tenant   = MakeTenant(tenantId);
        var user     = MakeUser(userId, tenantId, isActive: false);
        var command  = new LoginCommand(Email: "admin@test.com", Password: "Admin123!", TenantSlug: "admin", DeviceFingerprint: "fp-abc-123");

        var loginResult = new LoginResult { User = user, Tenant = tenant, ActiveSessions = new List<UserSession>() };

        _mockAuthRepo
            .Setup(x => x.GetLoginDataAsync("admin@test.com", "admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        var ex = await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.InnerException!.Message.Should().Contain("disponible");
    }
}
