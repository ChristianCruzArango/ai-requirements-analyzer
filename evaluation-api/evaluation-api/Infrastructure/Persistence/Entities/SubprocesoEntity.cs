namespace evaluation_api.Infrastructure.Persistence.Entities;

/// <summary>
/// Entidad que representa la tabla 'subproceso' en PostgreSQL
/// </summary>
public class SubprocesoEntity
{
    public int IdSubproceso { get; set; }
    public int IdProceso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
