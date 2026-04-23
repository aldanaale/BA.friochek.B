using BA.Backend.Application.Common.Interfaces;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace BA.Backend.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 10;
    private readonly ILogger<PasswordHasher> _logger;

    public PasswordHasher(ILogger<PasswordHasher> logger)
    {
        _logger = logger;
    }

    public string Hash(string password)
    {
        _logger.LogDebug("Hashing password with BCrypt");
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        return hashedPassword;
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
        {
            _logger.LogWarning("Password verification skipped: empty password or hash");
            return false;
        }

        try
        {
            var isValid = BCrypt.Net.BCrypt.Verify(password, hash);

            if (isValid)
            {
                _logger.LogDebug("Password verification successful");
            }
            else
            {
                _logger.LogWarning("Password verification failed");
            }

            return isValid;
        }
        catch (SaltParseException ex)
        {
            _logger.LogWarning(ex, "Invalid hash format during password verification");
            return false;
        }
    }
}
