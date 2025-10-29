using evaluation_api.Domain.Enums;

namespace evaluation_api.Domain.Models;

/// <summary>
/// Representa un caso de uso dentro de un subproceso
/// </summary>
public class CasoUso
{
    public int? IdCasoUso { get; set; }
    public int IdSubproceso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ActorPrincipal { get; set; }
    public TipoCasoUso TipoCasoUso { get; set; }
    public string? Precondiciones { get; set; }
    public string? Postcondiciones { get; set; }
    public string? CriteriosDeAceptacion { get; set; }

    public CasoUso()
    {
    }

    public CasoUso(
        int idSubproceso,
        string nombre,
        TipoCasoUso tipoCasoUso,
        string? descripcion = null,
        string? actorPrincipal = null,
        string? precondiciones = null,
        string? postcondiciones = null,
        string? criteriosDeAceptacion = null)
    {
        IdSubproceso = idSubproceso;
        Nombre = nombre;
        TipoCasoUso = tipoCasoUso;
        Descripcion = descripcion;
        ActorPrincipal = actorPrincipal;
        Precondiciones = precondiciones;
        Postcondiciones = postcondiciones;
        CriteriosDeAceptacion = criteriosDeAceptacion;
    }
}
