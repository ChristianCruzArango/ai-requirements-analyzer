namespace evaluation_api.Domain.Exceptions;

/// <summary>
/// Excepción para errores de base de datos
/// </summary>
public class DatabaseException : BaseException
{
    public DatabaseException(string message)
        : base(message, 500, "DATABASE_ERROR")
    {
    }

    public DatabaseException(string message, Exception innerException)
        : base(message, 500, "DATABASE_ERROR", innerException)
    {
    }
}
