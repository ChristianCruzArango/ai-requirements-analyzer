namespace evaluation_api.Infrastructure.Persistence.Entities;

/// <summary>
/// Entidad que representa la tabla 'proceso' en PostgreSQL
/// </summary>
public class ProcesoEntity
{
    public int IdProceso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
