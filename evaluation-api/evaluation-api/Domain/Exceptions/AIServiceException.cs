namespace evaluation_api.Domain.Exceptions;

/// <summary>
/// Excepción cuando falla el servicio de IA
/// </summary>
public class AIServiceException : BaseException
{
    public AIServiceException(string message)
        : base(message, 502, "AI_SERVICE_ERROR")
    {
    }

    public AIServiceException(string message, Exception innerException)
        : base(message, 502, "AI_SERVICE_ERROR", innerException)
    {
    }
}
