using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Auth.Handlers;
using BA.Backend.Application.Auth.DTOs;
using BA.Backend.Application.Exceptions;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Enums;
using BA.Backend.Domain.Repositories;
using BA.Backend.Application.Common.Interfaces;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BA.Backend.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<ITenantRepository>        _mockTenantRepo;
    private readonly Mock<IUserRepository>          _mockUserRepo;
    private readonly Mock<IUserSessionRepository>   _mockSessionRepo;
    private readonly Mock<ISessionService>          _mockSessionService;
    private readonly Mock<IJwtTokenService>         _mockJwtService;
    private readonly Mock<IPasswordHasher>          _mockHasher;
    private readonly IConfiguration                 _config;
    private readonly Mock<ILogger<LoginCommandHandler>> _mockLogger;
    private readonly LoginCommandHandler            _handler;

    public LoginCommandHandlerTests()
    {
        _mockTenantRepo     = new Mock<ITenantRepository>();
        _mockUserRepo       = new Mock<IUserRepository>();
        _mockSessionRepo    = new Mock<IUserSessionRepository>();
        _mockSessionService = new Mock<ISessionService>();
        _mockJwtService     = new Mock<IJwtTokenService>();
        _mockHasher         = new Mock<IPasswordHasher>();
        _mockLogger         = new Mock<ILogger<LoginCommandHandler>>();

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:UseRealDatabase", "false" },
                { "ConnectionStrings:DefaultConnection", "Server=.;Database=test;Trusted_Connection=True" }
            })
            .Build();

        _handler = new LoginCommandHandler(
            _mockTenantRepo.Object,
            _mockUserRepo.Object,
            _mockSessionRepo.Object,
            _mockSessionService.Object,
            _mockJwtService.Object,
            _mockHasher.Object,
            _config,
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

        _mockTenantRepo
            .Setup(x => x.GetBySlugAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _mockUserRepo
            .Setup(x => x.GetByEmailAsync("admin@test.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockHasher
            .Setup(x => x.Verify("Admin123!", user.PasswordHash))
            .Returns(true);

        _mockSessionRepo
            .Setup(x => x.GetActiveSessionsByUserAsync(userId))
            .ReturnsAsync(new List<UserSession>());

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

        _mockTenantRepo.Verify(x => x.GetBySlugAsync("admin", It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepo.Verify(x => x.GetByEmailAsync("admin@test.com", tenantId, It.IsAny<CancellationToken>()), Times.Once);
        _mockHasher.Verify(x => x.Verify("Admin123!", user.PasswordHash), Times.Once);
        _mockJwtService.Verify(x => x.GenerateToken(user, tenantId, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidTenant_ShouldThrowInvalidCredentialsException()
    {
        var command = new LoginCommand(Email: "admin@test.com", Password: "Admin123!", TenantSlug: "invalid-slug", DeviceFingerprint: "fp-abc-123");

        _mockTenantRepo
            .Setup(x => x.GetBySlugAsync("invalid-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var ex = await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.InnerException!.Message.Should().Contain("válida");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowInvalidCredentialsException()
    {
        var tenantId = Guid.NewGuid();
        var userId   = Guid.NewGuid();
        var tenant   = MakeTenant(tenantId);
        var user     = MakeUser(userId, tenantId);
        var command  = new LoginCommand(Email: "admin@test.com", Password: "WrongPassword!", TenantSlug: "admin", DeviceFingerprint: "fp-abc-123");

        _mockTenantRepo
            .Setup(x => x.GetBySlugAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _mockUserRepo
            .Setup(x => x.GetByEmailAsync("admin@test.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

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

        _mockTenantRepo
            .Setup(x => x.GetBySlugAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _mockUserRepo
            .Setup(x => x.GetByEmailAsync("admin@test.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var ex = await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.InnerException!.Message.Should().Contain("disponible");
    }
}
