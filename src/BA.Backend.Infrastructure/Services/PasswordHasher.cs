using BA.Backend.Application.Common.Interfaces;
using BCrypt.Net;

namespace BA.Backend.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        Console.WriteLine("Encriptando contraseña con BCrypt...");
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        return hashedPassword;
    }

    public bool Verify(string password, string hash)
    {
        Console.WriteLine("Verificando si la contraseña coincide con el hash...");
        var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        
        if (isValid)
        {
            Console.WriteLine("Contraseña correcta!");
        }
        else
        {
            Console.WriteLine("Contraseña incorrecta :(");
        }
        
        return isValid;
    }
}
