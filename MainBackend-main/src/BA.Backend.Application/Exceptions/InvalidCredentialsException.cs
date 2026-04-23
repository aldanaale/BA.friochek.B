namespace BA.Backend.Application.Exceptions;

/// <summary>
/// Se lanza cuando las credenciales de login son inválidas.
/// El mensaje público siempre es genérico para no revelar si el tenant o el usuario existen.
/// El motivo interno se almacena en InnerException para logging.
/// </summary>
public class InvalidCredentialsException : Exception
{
    private const string PublicMessage = "Email o contraseña inválidos.";

    /// <summary>
    /// Crea la excepción con el mensaje público estándar.
    /// </summary>
    public InvalidCredentialsException()
        : base(PublicMessage) { }

    /// <summary>
    /// Crea la excepción.
    /// El <paramref name="internalReason"/> se registra en el InnerException
    /// para logging pero NO se expone al cliente (seguridad: no revelar si
    /// el tenant o el usuario no existen).
    /// </summary>
    public InvalidCredentialsException(string internalReason)
        : base(PublicMessage, new Exception(internalReason)) { }
}
