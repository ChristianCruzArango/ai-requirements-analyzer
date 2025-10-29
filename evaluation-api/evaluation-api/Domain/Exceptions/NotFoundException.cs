namespace evaluation_api.Domain.Exceptions;

/// <summary>
/// Excepción cuando no se encuentra un recurso
/// </summary>
public class NotFoundException : BaseException
{
    public NotFoundException(string resource, object id)
        : base($"{resource} con ID {id} no encontrado", 404, "NOT_FOUND")
    {
    }

    public NotFoundException(string message)
        : base(message, 404, "NOT_FOUND")
    {
    }
}
