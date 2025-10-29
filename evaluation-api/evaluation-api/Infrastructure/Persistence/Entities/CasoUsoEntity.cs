namespace evaluation_api.Infrastructure.Persistence.Entities;

/// <summary>
/// Entidad que representa la tabla 'caso_uso' en PostgreSQL
/// </summary>
public class CasoUsoEntity
{
    public int IdCasoUso { get; set; }
    public int IdSubproceso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ActorPrincipal { get; set; }
    public int TipoCasoUso { get; set; }
    public string? Precondiciones { get; set; }
    public string? Postcondiciones { get; set; }
    public string? CriteriosDeAceptacion { get; set; }
}
