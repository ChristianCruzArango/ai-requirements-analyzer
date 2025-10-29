namespace evaluation_api.Domain.Models;

/// <summary>
/// Representa un subproceso dentro de un proceso principal
/// </summary>
public class Subproceso
{
    public int? IdSubproceso { get; set; }
    public int IdProceso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public List<CasoUso> CasosUso { get; set; } = new();

    public Subproceso()
    {
    }

    public Subproceso(int idProceso, string nombre, string? descripcion = null)
    {
        IdProceso = idProceso;
        Nombre = nombre;
        Descripcion = descripcion;
    }
}
