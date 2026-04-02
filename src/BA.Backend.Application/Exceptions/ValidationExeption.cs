namespace BA.Backend.Application.Exceptions;

public class ValidationExeption : Exception
{
    public Dictionary<string, string[]> Errors { get; set; }

    public ValidationExeption(Dictionary<string, string[]> errors)
        : base("Se han producido errores de validación.")
    {
        Errors = errors;
    }

    public ValidationExeption(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }
}